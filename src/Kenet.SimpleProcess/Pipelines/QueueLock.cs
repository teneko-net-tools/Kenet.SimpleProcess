namespace Kenet.SimpleProcess.Pipelines
{
    internal sealed class QueueLock
    {
        private object _instanceLock = new object();
        private int _totalDrawnPositions;
        private int _nextEnterablePosition = 1;
        private bool _isCanceled;

        /// <summary>
        /// Draws the next queue position.
        /// </summary>
        public int DrawPosition() =>
            Interlocked.Increment(ref _totalDrawnPositions);

        /// <summary>
        /// Acquires an exclusive lock when all previously queue positions have been released.
        /// </summary>
        /// <param name="position">
        /// A previously drawn queue position.
        /// </param>
        /// <exception cref="OperationCanceledException"></exception>
        public void Enter(int position)
        {
            Monitor.Enter(_instanceLock);

            while (true) {
                if (position < _nextEnterablePosition) {
                    throw new ArgumentOutOfRangeException(nameof(position), "The provided position exceeded the advanced queue position");
                }

                if (_isCanceled) {
                    throw new OperationCanceledException("The queue has been already closed");
                }

                if (position == _nextEnterablePosition) {
                    // The lock was or has been released
                    return;
                }

                Monitor.Wait(_instanceLock);
            }
        }

        public void Enter() =>
            Enter(DrawPosition());

        public void Exit()
        {
            Interlocked.Increment(ref _nextEnterablePosition);
            Monitor.PulseAll(_instanceLock);
            Monitor.Exit(_instanceLock);
        }

        public void Close()
        {
            _isCanceled = true;
            Monitor.PulseAll(_instanceLock);
        }
    }
}
