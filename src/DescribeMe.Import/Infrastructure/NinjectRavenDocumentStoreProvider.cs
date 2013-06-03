using DescribeMe.Core.Factories;
using Ninject.Activation;
using Raven.Client;

namespace DescribeMe.Import.Infrastructure
{
    public class NinjectRavenDocumentStoreProvider : Provider<IDocumentStore>
    {
        private readonly IDocumentStoreFactory _documentStoreFactory;
        
        public NinjectRavenDocumentStoreProvider(
            IDocumentStoreFactory documentStoreFactory)
        {
            _documentStoreFactory = documentStoreFactory;
        }

        protected override IDocumentStore CreateInstance(IContext ctx)
        {
            return _documentStoreFactory.MakeDocumentStore();
        }        
    }
}
