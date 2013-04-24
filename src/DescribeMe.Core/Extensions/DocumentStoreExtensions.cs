using System.Linq;
using System.Threading;
using Raven.Client;

namespace DescribeMe.Core.Extensions
{
    public static class DocumentStoreExtensions
    {
        public static IDocumentStore WaitForIndexingToFinish(this IDocumentStore documentStore, string[] indexNames = null)
        {
            if (indexNames != null && indexNames.Any())
            {
                while (documentStore.DatabaseCommands.GetStatistics().StaleIndexes.Any(indexNames.Contains))
                {
                    Thread.Sleep(1000);
                }
            }
            else
            {
                while (documentStore.DatabaseCommands.GetStatistics().StaleIndexes.Length > 0)
                {
                    Thread.Sleep(1000);
                }
            }

            return documentStore;
        }
    }
}