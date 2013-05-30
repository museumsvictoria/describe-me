using System;
using DescribeMe.Core.Commands;
using DescribeMe.Core.Config;
using DescribeMe.Core.Infrastructure;
using DescribeMe.Import.Config;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using Ninject.Extensions.Conventions;
using NinjectAdapter;
using Raven.Client;

namespace DescribeMe.Import
{
    class Program
    {
        private static IKernel _kernal;

        static void Main(string[] args)
        {
            _kernal = new StandardKernel();

            _kernal.Bind<IDocumentStore>().ToProvider<NinjectRavenDocumentStoreProvider>().InSingletonScope();
            _kernal.Bind<IDocumentSession>().ToProvider<NinjectRavenSessionProvider>();

            _kernal.Bind(x => x
                .FromAssemblyContaining(typeof(MessageBus), typeof(SignalRTarget))
                .SelectAllClasses()
                .BindAllInterfaces());

            _kernal.Bind<IServiceLocator>().ToMethod(x => ServiceLocator.Current);

            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(_kernal));

            var messageBus = _kernal.Get<IMessageBus>();

            // Setup application
            ServiceLocator.Current.GetInstance<IApplicationManager>()
                .SetupApplication();

            // Perform Import
            messageBus.Send(new ApplicationRunDataImportCommand { DateRun = DateTime.Now });

            _kernal.Get<IDocumentStore>().Dispose();
        }
    }
}