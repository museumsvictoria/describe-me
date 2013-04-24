namespace DescribeMe.Core.Infrastructure
{
    public interface IEventHandler<in T> where T : IEvent
    {
        void Handle(T @event);
    }
}