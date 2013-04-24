using System;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.Events
{
    public class ApplicationDataImportRunEvent : Event
    {
        public DateTime DateRun { get; private set; }

        public ApplicationDataImportRunEvent(
            DomainModel sender,
            DateTime dateRun)
            : base(sender, isLongRunning: true)
        {
            DateRun = dateRun;
        }
    }
}