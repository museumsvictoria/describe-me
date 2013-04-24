using System;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.Events
{
    public class ApplicationDataSyncRunEvent : Event
    {
        public DateTime DateRun { get; private set; }

        public DateTime LastDataImport { get; private set; }

        public ApplicationDataSyncRunEvent(
            DomainModel sender,             
            DateTime dateRun,
            DateTime lastDataImport)
            : base(sender, isLongRunning: true)
        {            
            DateRun = dateRun;
            LastDataImport = lastDataImport;
        }
    }
}