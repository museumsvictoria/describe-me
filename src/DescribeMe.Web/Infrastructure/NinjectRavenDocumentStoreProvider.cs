using System.Linq;
using DescribeMe.Core.Factories;
using DescribeMe.Core.Indexes;
using DescribeMe.Core.Services;
using NLog;
using Ninject.Activation;
using Raven.Abstractions.Data;
using Raven.Client;
using System;

namespace DescribeMe.Web.Infrastructure
{
    public class NinjectRavenDocumentStoreProvider : Provider<IDocumentStore>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IDocumentStoreFactory _documentStoreFactory;
        private readonly IStatisticsService _statisticsService;
        
        public NinjectRavenDocumentStoreProvider(
            IDocumentStoreFactory documentStoreFactory,
            IStatisticsService statisticsService)
        {
            _documentStoreFactory = documentStoreFactory;
            _statisticsService = statisticsService;
        }

        protected override IDocumentStore CreateInstance(IContext ctx)
        {
            var documentStore = _documentStoreFactory.MakeDocumentStore();

            // Register Changes
            documentStore.Changes()
              .ForIndex("Images/Statistics")
              .Subscribe(change =>
              {
                  if (change.Type == IndexChangeTypes.ReduceCompleted)
                  {
                      using (var documentSession = documentStore.OpenSession())
                      {
                          RavenQueryStatistics stats;
                          var statistics = documentSession
                              .Query<Images_Statistics.ReduceResult, Images_Statistics>()
                              .Statistics(out stats)
                              .FirstOrDefault();

                          if (!stats.IsStale)
                          {
                              log.Debug("Sending Statistics update to clients, {0} of {1} images described", statistics.DescibedImageCount, statistics.UnDescibedImageCount);

                              _statisticsService.UpdateStatistics(statistics);
                          }
                      }
                  }
              });

            return documentStore;
        }
    }
}
