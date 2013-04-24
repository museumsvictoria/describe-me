using DescribeMe.Core.DomainModels;
using Raven.Client;

namespace DescribeMe.Core.Infrastructure
{
    public class Repository<T> : IRepository<T> where T : DomainModel
    {
        private readonly IMessageBus _messageBus;
        private readonly IDocumentSession _documentSession;

        public Repository(
            IMessageBus messageBus, 
            IDocumentSession documentSession)
        {
            _messageBus = messageBus;
            _documentSession = documentSession;
        }

        public void Save(DomainModel domainModel)
        {
            this.Save(domainModel, false);
        }

        public void SaveAsync(DomainModel domainModel)
        {
            this.Save(domainModel, isAsync: true);
        }
        
        private void Save(DomainModel domainModel, bool isAsync)
        {
            // save to ravendb
            _documentSession.Store(domainModel);
            _documentSession.SaveChanges();

            // Publish events
            var events = domainModel.GetUnPublishedEvents();
            foreach (var @event in events)
            {
                if (isAsync)
                {
                    _messageBus.PublishAsync(@event);
                }
                else
                {
                    _messageBus.Publish(@event);
                }
            }
            domainModel.MarkEventsPublished();
        }
        
        public T GetById(string id)
        {
            // load ravendb
            return _documentSession.Load<T>(id);
        }
    }
}