using DescribeMe.Core.DesignByContract;
using DescribeMe.Core.DomainModels;

namespace DescribeMe.Core.Infrastructure
{
    public abstract class Event : IEvent
    {
        protected Event(
            DomainModel sender,
            bool isLongRunning = false)
        {
            Requires.IsNotNull(sender, "sender");
            
            Sender = sender;
            IsLongRunning = isLongRunning;
        }

        public DomainModel Sender { get; private set; }

        public bool IsLongRunning { get; private set; }
    }
}