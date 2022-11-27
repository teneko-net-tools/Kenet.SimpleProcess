namespace Kenet.SimpleProcess.Test.Infrastructure
{
    [CollectionDefinition(CollectionName, DisableParallelization = true)]
    public class KillingProcessesCollection : ICollectionFixture<SleepCommand>, ICollectionFixture<WriteCommand>
    {
        public const string CollectionName = "Killing Processes";
    }
}
