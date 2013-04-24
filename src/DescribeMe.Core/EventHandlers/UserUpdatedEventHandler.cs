using DescribeMe.Core.Events;
using DescribeMe.Core.Infrastructure;
using NLog;
using Raven.Abstractions.Data;
using Raven.Client;
using DescribeMe.Core.Extensions;

namespace DescribeMe.Core.EventHandlers
{
    public class UserUpdatedEventHandler : IEventHandler<UserUpdatedEvent>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IDocumentStore _documentStore;

        public UserUpdatedEventHandler(
            IDocumentStore documentStore)
        {
            _documentStore = documentStore;            
        }

        public void Handle(UserUpdatedEvent @event)
        {
            foreach (var staleIndex in _documentStore.DatabaseCommands.GetStatistics().StaleIndexes)
            {
                log.Debug("stale Index found {0}", staleIndex);
            }

            _documentStore
                .WaitForIndexingToFinish(new[]
                {
                    "Images/DescribedByUserId"
                })
                .DatabaseCommands
                .UpdateByIndex(
                "Images/DescribedByUserId",
                new IndexQuery { Query = string.Format(@"Id:{0}", @event.Sender.Id)}, 
                new []
                {
                    new PatchRequest
                        {
                            Type = PatchCommandType.Modify,
                            Name = "DescribedByUser",
                            Nested = new[]
                            {
                                new PatchRequest
                                {
                                    Type = PatchCommandType.Set,
                                    Name = "Name",
                                    Value = @event.Name
                                }
                            }
                        }
                },
                allowStale:false);
        }
    }
}