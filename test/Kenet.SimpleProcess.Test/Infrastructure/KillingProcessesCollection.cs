namespace Kenet.SimpleProcess.Test.Infrastructure
{
    //[CollectionDefinition(CollectionName, DisableParallelization = true)]
    [CollectionDefinition(CollectionName)]
    public class KillingProcessesCollection : ICollectionFixture<SleepCommand>, ICollectionFixture<WriteCommand>
    {
        public const string CollectionName = "Killing Processes";
    }
}
