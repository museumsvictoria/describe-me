using Raven.Client;

namespace DescribeMe.Core.Factories
{
    public interface IDocumentStoreFactory
    {
        IDocumentStore MakeDocumentStore();
    }
}