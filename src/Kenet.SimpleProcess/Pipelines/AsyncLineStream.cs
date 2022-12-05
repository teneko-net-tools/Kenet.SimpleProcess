using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using Nito.AsyncEx;

namespace Kenet.SimpleProcess.Pipelines
{
    internal sealed class AsyncLineStream : IDisposable
    {
        private delegate IEnumerable<ConsumedMemoryOwner<byte>> ReadLinesDelegate();
        private delegate bool TrySeekNewLineDelegate(out ReadOnlySequence<byte> line, out byte newLineLength);

        private static readonly byte _n = (byte)'\n';
        private static readonly byte _r = (byte)'\r';
        private static readonly byte[] _n_or_r = new byte[] { _n, _r };

        public AsyncCollection<ConsumedMemoryOwner<byte>> WrittenLines { get; }
        public bool IsCompleted => _writeTasks is null;
        public bool IsDisposed => _isDisposed == 1;

        private SequenceSegment _firstAnchorSegment;
        private SequenceSegment _firstSegment => _firstAnchorSegment.Next!;
        private SequenceSegment _lastSegment;
        private SequenceSegment _lastAnchorSegment;
        private ReadOnlySequence<byte> _sequence;
        private QueueLock _writeLock;
        private List<Task>? _writeTasks;
        private int _isDisposed;
        private bool _isCarriageReturnPending;
        private bool _isLastLinePending;

        public AsyncLineStream()
        {
            _firstAnchorSegment = new SequenceSegment();
            _lastAnchorSegment = new SequenceSegment();
            _lastSegment = _lastAnchorSegment;
            _firstAnchorSegment.ReplaceNextSegment(_lastAnchorSegment);
            _sequence = new ReadOnlySequence<byte>(_firstAnchorSegment, 0, _lastAnchorSegment, 0);
            _writeLock = new QueueLock();
            _writeTasks = new List<Task>();
            WrittenLines = new AsyncCollection<ConsumedMemoryOwner<byte>>();
        }

        [MemberNotNull(nameof(_writeTasks))]
        private void EnsureNotCompleted()
        {
            if (IsCompleted) {
                throw new AlreadyCompletedException("The stream has been already completed");
            }
        }

        private void EnsureNotDisposed()
        {
            if (IsDisposed) {
                throw new ObjectDisposedException(objectName: null, "The stream has been disposed");
            }
        }

        /// <summary>
        /// Advances prior or past <paramref name="line"/>.
        /// </summary>
        /// <param name="line">The sub-sequence of <see cref="_sequence"/>.</param>
        /// <param name="advancePastLine"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private void AdvanceTo(in ReadOnlySequence<byte> line, bool advancePastLine)
        {
            if (line.Equals(default)) {
                throw new ArgumentNullException(nameof(line));
            }

            var advancePosition = advancePastLine ? line.End : line.Start;
            var segmentToBeAdvancedTo = (SequenceSegment)advancePosition.GetObject()!;
            var segmentStartIndex = advancePosition.GetInteger();

            // Dispose all segments before that segment that contains the sequence
            _firstAnchorSegment.DisposeUntil(segmentToBeAdvancedTo);
            _firstAnchorSegment.ReplaceNextSegment(segmentToBeAdvancedTo);
            _firstSegment.IncrementOffset(segmentStartIndex);
        }

        /// <summary>
        /// Seeks for \n (Unix), \r\n (Window) or \r (MacOS) safely.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="newLineLength"></param>
        private bool TrySeekNewLine(out ReadOnlySequence<byte> line, out byte newLineLength)
        {
            var reader = new SequenceReader<byte>(_sequence);

            if (reader.TryAdvanceToAny(_n_or_r, advancePastDelimiter: false)) {
                if (reader.TryRead(out var next) && next == _n) {
                    newLineLength = 1; // \n
                } else if (!reader.TryRead(out next)) {
                    // We don't know if \r may be actually \r or \r\n
                    _isCarriageReturnPending = true;
                    goto @false;
                } else {
                    newLineLength = next == _n
                        ? (byte)2 // \r\n
                        : (byte)1; // \r
                }

                _isCarriageReturnPending = false;
                line = _sequence.Slice(0, reader.Consumed); // Consumed includes newline
                return true;
            }

            @false:
            line = ReadOnlySequence<byte>.Empty;
            newLineLength = 0;
            return false;
        }

        /// <summary>
        /// Seeks for \n (Unix), \r\n (Window) or \r (MacOS).
        /// </summary>
        /// <remarks>
        /// Will return line with newline even when the sequence only ends with \r.
        /// </remarks>
        /// <param name="line"></param>
        /// <param name="newLineLength"></param>
        private bool TrySeekNewLineUnsafe(out ReadOnlySequence<byte> line, out byte newLineLength)
        {
            var reader = new SequenceReader<byte>(_sequence);

            if (reader.TryAdvanceToAny(_n_or_r, advancePastDelimiter: false)) {
                newLineLength = reader.TryRead(out var next) && next == _r && reader.TryRead(out next) && next == _n
                    ? (byte)2
                    : (byte)1;

                line = _sequence.Slice(0, reader.Consumed); // Consumed includes newline
                return true;
            }

            line = ReadOnlySequence<byte>.Empty;
            newLineLength = 0;
            return false;
        }

        private bool TryReadLine(out ConsumedMemoryOwner<byte> memoryOwner, ReadOnlySequence<byte> line, int newLineLength, bool advanceStream = true)
        {
            if (!_isLastLinePending && line.Length == 0 && line.Start.Equals(_sequence.Start) && line.End.Equals(_sequence.End)) {
                memoryOwner = ConsumedMemoryOwner<byte>.Empty;
                return false;
            }

            var longLineLength = line.Length - newLineLength;

            if (longLineLength > int.MaxValue) {
                throw new OverflowException($"Line length exceeded the total number of {int.MaxValue}");
            }

            var lineLength = (int)longLineLength;
            var lineMemoryOwner = MemoryPool<byte>.Shared.Rent(lineLength);
            line.Slice(0, lineLength).CopyTo(lineMemoryOwner.Memory.Span);

            if (advanceStream) {
                AdvanceTo(line, advancePastLine: true);
            }

            // This boolean wants to solve the following problem:
            // Imagine writing the line: "Example\n", then in case no write follows
            // anymore, a empty line MUST follow. This is handled in Complete(bool).
            _isLastLinePending = true;
            memoryOwner = new ConsumedMemoryOwner<byte>(lineMemoryOwner, lineLength);
            return true;
        }

        private void AppendMemory(ConsumedMemoryOwner<byte> memoryOwner)
        {
            var nextSegment = new SequenceSegment(memoryOwner, memoryOwner.ConsumedCount);

            // If no first segment has been set, we do so
            if (ReferenceEquals(_firstAnchorSegment.Next, _lastAnchorSegment)) {
                _firstAnchorSegment.ReplaceNextSegment(nextSegment);
                _lastSegment = nextSegment;
                // But now we have to correct the new last segment to update its next segment being the end segment
                _lastSegment.ReplaceNextSegment(_lastAnchorSegment);
            }
            // Otherwise we simply append next segment to last segment
            else {
                // Replace next segment of current last segment internally
                _lastSegment.ReplaceNextSegment(nextSegment);
                // Now update our local reference too
                _lastSegment = nextSegment;
                _lastSegment.ReplaceNextSegment(_lastAnchorSegment);
            }
        }

        private IEnumerable<ConsumedMemoryOwner<byte>> ReadLines(TrySeekNewLineDelegate trySeekNewLine)
        {
            while (trySeekNewLine(out var line, out var newLineLength)) {
                if (TryReadLine(out var lineOwner, line, newLineLength, advanceStream: true)) {
                    yield return lineOwner;
                }
            }
        }

        private IEnumerable<ConsumedMemoryOwner<byte>> ReadLines() =>
            ReadLines(TrySeekNewLine);

        /// <summary>
        /// Accepts the memory to extract lines from it.
        /// </summary>
        /// <param name="memoryOwner"></param>
        /// <param name="readLines"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void Write(ConsumedMemoryOwner<byte> memoryOwner, ReadLinesDelegate readLines)
        {
            lock (_writeLock) {
                EnsureNotDisposed();
                EnsureNotCompleted();

                var position = _writeLock.DrawPosition();

                // TODO: Consider background thread
                _writeTasks.Add(Task.Run(() => {
                    _writeLock.Enter(position);

                    try {
                        AppendMemory(memoryOwner);

                        foreach (var line in readLines()) {
                            WrittenLines.Add(line);
                        }
                    } catch (Exception error) {
                        WrittenLines.Add(new ConsumedMemoryOwner<byte>(new ErroneousMemoryOwner<byte>(error), 0));
                    } finally {
                        _writeLock.Exit();
                    }
                }));
            }
        }

        /// <inheritdoc cref="Write(ConsumedMemoryOwner{byte}, ReadLinesDelegate)"/>
        public void Write(ConsumedMemoryOwner<byte> memoryOwner) =>
            Write(memoryOwner, ReadLines);

        private async Task CompleteAsync(bool synchronous)
        {
            List<Task> writeTasks;

            lock (_writeLock) {
                EnsureNotDisposed();
                EnsureNotCompleted();

                if (_writeTasks is null) {
                    return;
                }

                writeTasks = _writeTasks;
                _writeTasks = null;
            }

            if (synchronous) {
                await Task.WhenAll(writeTasks).ConfigureAwait(false);
            } else {
                Task.WaitAll(writeTasks.ToArray());
            }

            // We write directly to underlying buffer
            try {
                if (_isCarriageReturnPending
                    && TrySeekNewLineUnsafe(out var lineSequence, out var newLineLength)
                    && TryReadLine(out var line, _sequence, newLineLength, advanceStream: true)) {
                    if (synchronous) {
                        WrittenLines.Add(line);
                    } else {
                        await WrittenLines.AddAsync(line).ConfigureAwait(false);
                    }
                }

                if (TryReadLine(out line, _sequence, newLineLength: 0, advanceStream: false)) {
                    if (synchronous) {
                        WrittenLines.Add(line);
                    } else {
                        await WrittenLines.AddAsync(line).ConfigureAwait(false);
                    }
                }
            } catch (Exception error) {
                var memoryOwner = new ConsumedMemoryOwner<byte>(new ErroneousMemoryOwner<byte>(error), 0);

                if (synchronous) {
                    WrittenLines.Add(memoryOwner);
                } else {
                    await WrittenLines.AddAsync(memoryOwner).ConfigureAwait(false);
                }
            }

            WrittenLines.CompleteAdding();
        }

        public Task CompleteAsync() =>
            CompleteAsync(synchronous: false);

        public void Complete() =>
            CompleteAsync(synchronous: true).GetAwaiter().GetResult();

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1) {
                return;
            }

            // Ensures we "Cancel" waiting operations of WrittenLines, if not done previously
            WrittenLines.CompleteAdding();

            _firstSegment?.DisposeUntil(_lastAnchorSegment);
            _firstAnchorSegment = null!;
            _lastSegment = null!;
            _lastAnchorSegment = null!;
        }

        private class ErroneousMemoryOwner<T> : IMemoryOwner<T>
        {
            public Memory<T> Memory {
                get {
                    Rethrow();
                    // Make compiler happy
                    return Memory<T>.Empty;
                }
            }

            private readonly Exception _exception;

            internal ErroneousMemoryOwner(Exception exception) =>
                _exception = exception ?? throw new ArgumentNullException(nameof(exception));

            private void Rethrow() =>
                ExceptionDispatchInfo.Capture(_exception).Throw();

            public void Dispose() =>
                Rethrow();
        }
    }
}
