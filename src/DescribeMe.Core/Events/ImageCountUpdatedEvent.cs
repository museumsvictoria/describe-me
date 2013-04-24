using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.Events
{
    public class ImageCountUpdatedEvent : Event
    {
        public ImageCountUpdatedEvent(
            DomainModel sender)
            : base(sender)
        {
        }
    }
}