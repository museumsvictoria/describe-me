using DescribeMe.Core.DomainModels;

namespace DescribeMe.Core.Infrastructure
{
    public interface IEvent
    {
        DomainModel Sender { get; }

        bool IsLongRunning { get; }
    }
}