using DescribeMe.Core.Config;
using DescribeMe.Core.Indexes;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Raven.Client.Indexes;

namespace DescribeMe.Core.Factories
{
    public class DocumentStoreFactory : IDocumentStoreFactory
    {
        private readonly IConfigurationManager _configurationManager;

        public DocumentStoreFactory(
            IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public IDocumentStore MakeDocumentStore()
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

            return documentStore;
        }
    }
}