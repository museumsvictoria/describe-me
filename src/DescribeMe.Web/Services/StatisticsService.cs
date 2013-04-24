using DescribeMe.Core.Indexes;
using DescribeMe.Core.Services;
using DescribeMe.Web.Hubs;
using DescribeMe.Web.ViewModels;
using SignalR;
using SignalR.Hubs;

namespace DescribeMe.Web.Services
{
    public sealed class StatisticsService : IStatisticsService
    {
        private readonly IHubContext _hubContext;

        public StatisticsService()
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<StatisticsHub>();
        }
        
        public void UpdateStatistics(Images_Statistics.ReduceResult statistics)
        {
            var statisticsViewModel = new StatisticsViewModel
            {
                DescribedImageCount = statistics.DescibedImageCount,
                UnDescribedImageCount = statistics.UnDescibedImageCount,
                ApprovedImageCount = statistics.ApprovedImageCount,
                UnApprovedImageCount = statistics.UnApprovedImageCount
            };

            _hubContext.Clients.updateStatistics(
                statisticsViewModel.DescribedImageCount,
                statisticsViewModel.DescribedImageCountAsPercentage,
                statisticsViewModel.ApprovedImageCount,
                statisticsViewModel.ApprovedImageCountAsPercentage);
        }
    }
}