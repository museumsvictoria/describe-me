using DescribeMe.Core.Config;
using DescribeMe.Core.Indexes;
using Ninject.Activation;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Raven.Client.Indexes;

namespace DescribeMe.Core.Infrastructure
{
    public class NinjectRavenDocumentStoreProvider : Provider<IDocumentStore>
    {
        private readonly IConfigurationManager _configurationManager;
        
        public NinjectRavenDocumentStoreProvider(
            IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        protected override IDocumentStore CreateInstance(IContext ctx)
        {
            var documentStore = new DocumentStore
            {
                Url = _configurationManager.DatabaseUrl()
            };

            var hasDefaultDatabase = !string.IsNullOrWhiteSpace(_configurationManager.DatabaseName());

            if (hasDefaultDatabase)
            {
                documentStore.DefaultDatabase = _configurationManager.DatabaseName();
            }

            documentStore.Initialize();

            if (hasDefaultDatabase)
            {
                documentStore.DatabaseCommands.EnsureDatabaseExists(_configurationManager.DatabaseName());
            }
            
            // Add our indexes
            IndexCreation.CreateIndexes(typeof(Images_Statistics).Assembly, documentStore);

            //Uncomment for debug purposes only
            //Raven.Client.MvcIntegration.RavenProfiler.InitializeFor(documentStore);

            return documentStore;
        }
    }
}
