using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.Events
{
    public class UserUpdatedEvent : Event
    {
        public string Name { get; private set; }

        public UserUpdatedEvent(
            DomainModel sender,
            string name)
            : base(sender)
        {
            Name = name;
        }
    }
}