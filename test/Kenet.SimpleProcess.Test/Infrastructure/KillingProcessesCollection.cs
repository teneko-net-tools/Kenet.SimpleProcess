namespace Kenet.SimpleProcess.Test.Infrastructure
{
    [CollectionDefinition(CollectionName)]
    public class KillingProcessesCollection : ICollectionFixture<DummyCommand>
    {
        public const string CollectionName = "Killing Processes";
    }
}
