using System;
using System.Collections.Generic;
using System.Web.Mvc;
using DescribeMe.Core.Commands;
using DescribeMe.Core.Config;
using DescribeMe.Core.Infrastructure;
using NLog;
using NLog.Targets;
using System.Linq;
using DescribeMe.Core.Extensions;

namespace DescribeMe.Web.Controllers
{    
    [Authorize(Roles="administrator")]
    public class AdminController : Controller
    {
        private readonly IMessageBus _messageBus;
        
        public AdminController(
            IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }
        
        public ActionResult Index()
        {
            return View(GetLogTail());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RunDataImport()
        {
            _messageBus.SendAsync(new ApplicationRunDataImportCommand { DateRun = DateTime.Now });

            if(Request.IsAjaxRequest())
            {
                return new JsonResult { Data = "Success"} ;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelDataImport()
        {
            _messageBus.SendAsync(new ApplicationCancelDataImportCommand());

            if (Request.IsAjaxRequest())
            {
                return new JsonResult { Data = "Success" };
            }

            return RedirectToAction("Index");
        }

        // Move to builders
        private IEnumerable<string> GetLogTail()
        {
            var logTail = new List<string>();
            var target = LogManager.Configuration.AllTargets.FirstOrDefault(x => x.Name == "consoleFile") as FileTarget;
            var fileName = (target == null) ? string.Empty : target.FileName.Render(new LogEventInfo { Level = LogLevel.Debug });
            var archiveFilename = target.ArchiveFileName.Render(new LogEventInfo {Level = LogLevel.Debug}).Replace("{#}", "0");

            if(!string.IsNullOrWhiteSpace(fileName) && System.IO.File.Exists(fileName))
            {
                logTail = System.IO.File.ReadLines(fileName).Last(Constants.MaxLinesConsoleLogTail).ToList();
            }

            // Not enuff lines to fill the console so look in the archive to get the rest
            if (logTail.Count < Constants.MaxLinesConsoleLogTail
                && !string.IsNullOrWhiteSpace(archiveFilename)
                && System.IO.File.Exists(archiveFilename))
            {
                var archiveLogTail = System.IO.File.ReadLines(archiveFilename).Last(Constants.MaxLinesConsoleLogTail - logTail.Count).ToList();
                
                logTail.InsertRange(0, archiveLogTail);
            }

            return logTail;
        }
    }
}