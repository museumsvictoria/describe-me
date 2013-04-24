using System.Web;
using DescribeMe.Web.Hubs;
using NLog;
using NLog.Targets;
using SignalR;
using SignalR.Hubs;

namespace DescribeMe.Web.Config
{
    [Target("SignalR")]
    public class SignalRTarget : TargetWithLayout
    {
        private readonly IHubContext _hubContext;

        public SignalRTarget()
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<LoggingHub>();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var logMessage = Layout.Render(logEvent);

            _hubContext.Clients.sendLogMessage(HttpUtility.HtmlEncode(logMessage));
        }
    }
}
