using System.Collections.Generic;
using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.DomainModels
{
    public abstract class DomainModel
    {
        private readonly List<IEvent> _events = new List<IEvent>();

        public string Id { get; set; }

        public IEnumerable<IEvent> GetUnPublishedEvents()
        {
            return _events;
        }

        public void MarkEventsPublished()
        {
            _events.Clear();
        }

        protected void ApplyEvent<T>(T @event) where T : IEvent
        {           
            _events.Add(@event);
        }
    }
}