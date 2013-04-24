using NLog;
using SignalR.Hubs;

namespace DescribeMe.Web.Hubs
{
    public class LoggingHub : Hub
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public void SendLogEvent(string logMessage)
        {
            log.Debug(logMessage);
        }
    }
}