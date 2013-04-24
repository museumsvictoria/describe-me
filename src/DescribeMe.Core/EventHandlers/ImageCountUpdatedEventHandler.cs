using System;
using System.Diagnostics;
using DescribeMe.Core.Config;
using DescribeMe.Core.Events;
using DescribeMe.Core.Indexes;
using DescribeMe.Core.Infrastructure;
using DescribeMe.Core.Services;
using NLog;
using Raven.Client;
using System.Linq;

namespace DescribeMe.Core.EventHandlers
{
    public class ImageCountUpdatedEventHandler : IEventHandler<ImageCountUpdatedEvent>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IStatisticsService _statisticsService;
        private readonly IDocumentSession _documentSession;

        public ImageCountUpdatedEventHandler(
            IStatisticsService statisticsService,
            IDocumentSession documentSession)
        {
            _statisticsService = statisticsService;
            _documentSession = documentSession;            
        }

        public void Handle(ImageCountUpdatedEvent @event)
        {
            Images_Statistics.ReduceResult statistics;

            var stopwatch = Stopwatch.StartNew();
            try
            {
                // Wait as long as we can for non stale data as of now
                statistics = _documentSession
                                 .Query<Images_Statistics.ReduceResult, Images_Statistics>()
                                 .Customize(x => x.WaitForNonStaleResultsAsOfNow(TimeSpan.FromSeconds(Constants.WaitForNonStaleResultsTimeout)))
                                 .FirstOrDefault() ?? new Images_Statistics.ReduceResult();

                stopwatch.Stop();

                log.Debug("Non stale image statistics retrieved, described count:{0}, undescribed count:{1}, time elapsed:{2}ms", statistics.DescibedImageCount, statistics.UnDescibedImageCount, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception)
            {
                // Query has timed out, but we dont care about stale data, just return something!
                statistics = _documentSession
                                 .Query<Images_Statistics.ReduceResult, Images_Statistics>()
                                 .FirstOrDefault() ?? new Images_Statistics.ReduceResult();

                log.Debug("Timeout ({0}s) on imagestatistics has been hit, returning stale results", Constants.WaitForNonStaleResultsTimeout);
            }

            _statisticsService.UpdateStatistics(statistics);
        }
    }
}