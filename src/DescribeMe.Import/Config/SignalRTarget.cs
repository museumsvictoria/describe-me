using DescribeMe.Core.Config;
using Microsoft.Practices.ServiceLocation;
using NLog;
using NLog.Targets;
using SignalR.Client.Hubs;

namespace DescribeMe.Import.Config
{
    [Target("SignalR")]
    public class SignalRTarget : TargetWithLayout
    {
        private readonly IHubProxy _loggingHub;

        public SignalRTarget()
        {
            var configurationManager = ServiceLocator.Current.GetInstance<IConfigurationManager>();

            var s = configurationManager.SiteUrl();

            var hubConnection = new HubConnection(s);

            _loggingHub = hubConnection.CreateProxy("LoggingHub");

            hubConnection.Start().Wait();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var logMessage = Layout.Render(logEvent);

            _loggingHub.Invoke("SendLogEvent", logMessage);
        }
    }
}