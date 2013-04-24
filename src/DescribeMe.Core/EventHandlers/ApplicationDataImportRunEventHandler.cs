using System;
using System.Collections.Generic;
using System.Diagnostics;
using DescribeMe.Core.Config;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Events;
using DescribeMe.Core.Factories;
using DescribeMe.Core.Infrastructure;
using IMu;
using NLog;
using Raven.Client;
using System.Linq;

namespace DescribeMe.Core.EventHandlers
{
    public class ApplicationDataImportRunEventHandler : IEventHandler<ApplicationDataImportRunEvent>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IDocumentStore _documentStore;
        private readonly IImageFactory _imageFactory;
        private readonly IConfigurationManager _configurationManager;

        private Session _session;
        private List<Image> _imagesToImport;

        public ApplicationDataImportRunEventHandler(
            IDocumentStore documentStore,
            IImageFactory imageFactory,
            IConfigurationManager configurationManager)
        {
            _documentStore = documentStore;
            _imageFactory = imageFactory;
            _configurationManager = configurationManager;

            _imagesToImport = new List<Image>();
        }

        public void Handle(ApplicationDataImportRunEvent @event)
        {
            var s = _configurationManager.EmuPort();
            bool hasFailed = false;
            try
            {                
                log.Debug("Data import begining");

                // Connect to Imu
                log.Debug("Connecting to Imu server: {0}:{1}", _configurationManager.EmuHost(), _configurationManager.EmuPort());
                
                _session = new Session(_configurationManager.EmuHost(), _configurationManager.EmuPort());
                _session.Connect();

                RunDataImport();
                RunFileImport();
            }
            catch (Exception exception)
            {
                hasFailed = true;
                log.Debug(exception);
            }

            // Possibly should be an event fired via repo.
            using (var documentSession = _documentStore.OpenSession())
            {
                var application = documentSession.Load<Application>(Constants.ApplicationId);

                if (application.DataImportCancelled || hasFailed)
                {
                    log.Debug("Data import finished (cancelled or failed), image count ({0})", _imagesToImport.Count);
                    application.DataImportFinished();
                }
                else
                {
                    log.Debug("Data import finished succesfully, image count ({0})", _imagesToImport.Count);
                    application.DataImportSuccess(@event.DateRun);
                }

                documentSession.SaveChanges();
            }
        }

        private void RunDataImport()
        {
            // Get search ready
            var catalogue = new Module("ecatalogue", _session);
            var search = new Terms();
            search.Add("ColCategory", "History & Technology");
            search.Add("MdaDataSets_tab", "History & Technology Collections Online");            
            search.Add("AdmPublishWebNoPassword", "Yes");            

            // Perform search
            var stopwatch = Stopwatch.StartNew();
            var hits = catalogue.FindTerms(search);
            stopwatch.Stop();
            log.Debug("Finished Catalogue Emu Search in {0:#,#} ms. {1} Hits", stopwatch.ElapsedMilliseconds, hits);

            var columns = new[] {
                                   "irn",
                                   "ColRegPrefix",
                                   "ColRegNumber",
                                   "ColRegPart",
                                   "ColDiscipline",
                                   "ColCategory",
                                   "ColTypeOfItem",
                                   "ClaObjectName",
                                   "ClaObjectSummary",
                                   "ClaPrimaryClassification",
                                   "ClaSecondaryClassification",
                                   "ClaTertiaryClassification",
                                   "SubHistoryTechSignificance",
                                   "DesPhysicalDescription",                                   
                                   "Con1Description",
                                   "SubSubjects_tab",
                                   "images=MulMultiMediaRef_tab.(irn,MulTitle,MulDescription,MulIdentifier,AdmPublishWebNoPassword)"
                               };

            var count = 0;
            stopwatch = Stopwatch.StartNew();

            while (true)
            {
                using (var documentSession = _documentStore.OpenSession())
                {
                    if (documentSession.Load<Application>(Constants.ApplicationId).DataImportCancelled)
                    {
                        log.Debug("Cancel command recieved stopping Data import");
                        return;
                    }

                    var results = catalogue.Fetch("start", count, Constants.DataBatchSize, columns);

                    if (results.Count == 0)
                        break;

                    var images = results.Rows
                        .SelectMany(x => _imageFactory.MakeImages(x))
                        .Where(x => !_imagesToImport.Any(y => x.Id == y.Id));

                    _imagesToImport.AddRange(images);

                    count += results.Count;
                    log.Debug("Data import progress... {0}/{1}", count, hits);
                }
            }

            stopwatch.Stop();
            log.Debug("Data import finished, total Time: {0:0.00} Mins", stopwatch.Elapsed.TotalMinutes); 
        }

        private void RunFileImport()
        {
            var count = 0;
            var removedImageCount = 0;
            var successImageCount = 0;
            var stopwatch = Stopwatch.StartNew();

            log.Debug("Begining File import on {0} images", _imagesToImport.Count);

            while(true)
            {
                using (var documentSession = _documentStore.OpenSession())
                {
                    if (documentSession.Load<Application>(Constants.ApplicationId).DataImportCancelled)
                    {
                        log.Debug("Cancel command recieved stopping File import");
                        return;
                    }

                    var imageBatch = _imagesToImport
                        .Skip(count)
                        .Take(Constants.FileBatchSize)
                        .ToList();

                    // Stop if we have no more images
                    if (imageBatch.Count == 0)
                        break;

                    // Perform fetch image on images
                    foreach (var image in imageBatch)
                    {
                        if(_imageFactory.TryFetchImage(_session, image, false))
                        {
                            documentSession.Store(image);
                            successImageCount++;
                        }
                        else
                        {
                            removedImageCount++;
                        }
                    }

                    documentSession.SaveChanges();
                    count += imageBatch.Count;
                    log.Debug("File import progress... {0}/{1}", count, _imagesToImport.Count);                    
                }
            }

            stopwatch.Stop();
            log.Debug("File import finished, total Time: {0:0.00} Mins, success:{1}, removed due to errors:{2}", stopwatch.Elapsed.TotalMinutes, successImageCount, removedImageCount);
        }        
    }
}