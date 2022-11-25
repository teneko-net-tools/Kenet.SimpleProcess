namespace Kenet.SimpleProcess.Test.Infrastructure
{
    [CollectionDefinition(CollectionName)]
    public class KillingProcessesCollection : ICollectionFixture<SleepCommand>
    {
        public const string CollectionName = "Killing Processes";
    }
}
