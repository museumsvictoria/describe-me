namespace DescribeMe.Core.Services
{
    public interface IStatisticsService
    {
        void UpdateStatistics(Indexes.Images_Statistics.ReduceResult statistics);
    }
}