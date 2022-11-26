namespace Kenet.SimpleProcess.Pipelines
{
    internal sealed class TicketLock
    {
        private object _instanceLock;
        private int _ticketsCount = 0;
        private int _ticketToRide = 1;
        private bool _canceled;

        public TicketLock() =>
            _instanceLock = new object();

        /// <summary>
        /// Draws the next ticket.
        /// </summary>
        /// <returns></returns>
        public int DrawTicket() =>
            Interlocked.Increment(ref _ticketsCount);

        public void Enter(int ticket)
        {
            Monitor.Enter(_instanceLock);

            while (true) {
                if (_canceled) {
                    throw new OperationCanceledException("The queue lock has been canceled.");
                }

                if (ticket == _ticketToRide) {
                    // The lock was or has been released
                    return;
                }

                Monitor.Wait(_instanceLock);
            }
        }

        public void Exit()
        {
            Interlocked.Increment(ref _ticketToRide);
            Monitor.PulseAll(_instanceLock);
            Monitor.Exit(_instanceLock);
        }

        public void Cancel()
        {
            _canceled = true;
            Monitor.PulseAll(_instanceLock);
        }
    }
}
