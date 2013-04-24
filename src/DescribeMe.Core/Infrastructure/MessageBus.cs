using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using NLog;
using ReflectionMagic;
using DescribeMe.Core.Extensions;

namespace DescribeMe.Core.Infrastructure
{
    public class MessageBus : IMessageBus
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IServiceLocator _serviceLocator;

        public MessageBus(
            IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public void Send<T>(T command) where T : ICommand
        {
            var handler = _serviceLocator.GetAllInstances<ICommandHandler<T>>();

            if (handler == null || !handler.Any())
            {
                throw new CommandHandlerNotFoundException(typeof(T));
            }

            if (handler.Count() != 1)
            {
                throw new MultipleCommandHandlersFoundException(typeof(T));
            }

            log.Debug("Sending Command: {0}", command.GetType().Name);
            handler.First().Handle(command);            
        }

        public void SendAsync<T>(T command) where T : ICommand
        {
            var handler = _serviceLocator.GetAllInstances<ICommandHandler<T>>();

            if (handler == null || !handler.Any())
            {
                throw new CommandHandlerNotFoundException(typeof(T));
            }

            if (handler.Count() != 1)
            {
                throw new MultipleCommandHandlersFoundException(typeof(T));
            }

            log.Debug("Sending Async Command: {0}", command.GetType().Name);
            Task.Factory.StartNew(state => handler.First().Handle(command), command.GetType().Name).LogExceptions();
        }

        public void Publish<T>(T @event) where T : IEvent
        {
            var type = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
            
            var handlers = ServiceLocator.Current.GetAllInstances(type);

            foreach (var handler in handlers)
            {
                log.Debug("Publishing Event: {0}", @event.GetType());
                handler.AsDynamic().Handle(@event);
            }
        }

        public void PublishAsync<T>(T @event) where T : IEvent
        {
            var type = typeof(IEventHandler<>).MakeGenericType(@event.GetType());

            var handlers = ServiceLocator.Current.GetAllInstances(type);

            foreach (var handler in handlers)
            {
                var asyncHandler = handler;

                log.Debug("Publishing Async Event: {0}", @event.GetType().Name);
                
                if(@event.IsLongRunning)
                {
                    Task.Factory.StartNew(state => asyncHandler.AsDynamic().Handle(@event), @event.GetType().Name, TaskCreationOptions.LongRunning).LogExceptions();                    
                }
                else
                {
                    Task.Factory.StartNew(state => asyncHandler.AsDynamic().Handle(@event), @event.GetType().Name).LogExceptions();
                }
            }
        }
    }
}