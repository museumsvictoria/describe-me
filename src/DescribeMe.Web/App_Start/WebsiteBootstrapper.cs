using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DescribeMe.Core.Config;
using DescribeMe.Core.Infrastructure;
using DescribeMe.Web.Config;
using DescribeMe.Web.Infrastructure;
using Microsoft.Practices.ServiceLocation;
using NinjectAdapter;
using System;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using Ninject.Extensions.Conventions;
using Raven.Client;
using SignalR;
using SignalR.Ninject;

[assembly: WebActivator.PreApplicationStartMethod(typeof(DescribeMe.Web.App_Start.WebsiteBootstrapper), "PreStart")]
[assembly: WebActivator.PostApplicationStartMethod(typeof(DescribeMe.Web.App_Start.WebsiteBootstrapper), "PostStart")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(DescribeMe.Web.App_Start.WebsiteBootstrapper), "Stop")]

namespace DescribeMe.Web.App_Start
{
    public static class WebsiteBootstrapper
    {        
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();
        private static IKernel _kernal;

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void PreStart()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(SuppressFormsAuthenticationRedirectModule));

            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            _kernal = new StandardKernel();

            _kernal.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            _kernal.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            RegisterServices(_kernal);
            return _kernal;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            // Raven Db Bindings
            kernel.Bind<IDocumentStore>().ToProvider<NinjectRavenDocumentStoreProvider>().InSingletonScope();
            kernel.Bind<IDocumentSession>().ToProvider<NinjectRavenSessionProvider>();
            
            // Application manager
            kernel.Bind<IApplicationManager>().To<ApplicationManager>().InSingletonScope();

            // The rest of our bindings
            kernel.Bind(x => x
                .FromAssemblyContaining(typeof(MessageBus), typeof(WebsiteBootstrapper))
                .SelectAllClasses()
                .Excluding<ApplicationManager>()
                .BindAllInterfaces());

            kernel.Bind<IServiceLocator>().ToMethod(x => ServiceLocator.Current);

            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(kernel));
        }

        public static void PostStart()
        {
            GlobalHost.DependencyResolver = new NinjectDependencyResolver(_kernal);

            RouteTable.Routes.MapHubs();

            AreaRegistration.RegisterAllAreas();

            // Register Global Filters
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            // Register Routes
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Register our JS and CSS Bundles
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Register our OAuth/OpenId providers
            AuthConfig.RegisterAuth();

            // Perform Application setup
            ServiceLocator.Current.GetInstance<IApplicationManager>().SetupApplication();

            BundleTable.EnableOptimizations = true;
        }
    }
}