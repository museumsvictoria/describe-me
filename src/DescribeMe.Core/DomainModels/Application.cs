using System;
using DescribeMe.Core.Config;
using DescribeMe.Core.Events;

namespace DescribeMe.Core.DomainModels
{
    public class Application : DomainModel
    {
        public DateTime LastDataImport { get; private set; }

        public bool DataImportRunning { get; private set; }

        public bool DataImportCancelled { get; private set; }

        public Application()
        {
            Id = Constants.ApplicationId;
        }

        public void RunDataImport(DateTime dateRun)
        {
            if (!DataImportRunning)
            {                
                DataImportRunning = true;

                if (LastDataImport == default(DateTime))
                {
                    // First time import has run, perform full import.
                    ApplyEvent(new ApplicationDataImportRunEvent(this, dateRun));
                }
                else
                {
                    // Import has run before, perform sync instead.
                    ApplyEvent(new ApplicationDataSyncRunEvent(this, dateRun, LastDataImport));
                }
            }
        }
        
        public void DataImportFinished()
        {
            DataImportRunning = false;
            DataImportCancelled = false;
        }

        public void DataImportSuccess(DateTime dateCompleted)
        {
            DataImportRunning = false;
            DataImportCancelled = false;
            LastDataImport = dateCompleted;
        }

        public void CancelDataImport()
        {
            if (DataImportRunning)
            {
                DataImportCancelled = true;
            }
        }
    }
}