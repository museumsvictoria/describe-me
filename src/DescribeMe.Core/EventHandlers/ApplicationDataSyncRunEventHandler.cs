using System;
using System.Collections.Generic;
using System.Diagnostics;
using DescribeMe.Core.Config;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Events;
using DescribeMe.Core.Extensions;
using DescribeMe.Core.Factories;
using DescribeMe.Core.Indexes;
using DescribeMe.Core.Infrastructure;
using IMu;
using NLog;
using Raven.Client;
using System.Linq;
using Raven.Client.Linq;

namespace DescribeMe.Core.EventHandlers
{
    public class ApplicationDataSyncRunEventHandler : IEventHandler<ApplicationDataSyncRunEvent>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IDocumentStore _documentStore;
        private readonly IImageFactory _imageFactory;
        private readonly IConfigurationManager _configurationManager;

        private Session _session;
        private List<Image> _imagesToImport;
        private List<string> _imagesToRetry;

        public ApplicationDataSyncRunEventHandler(
            IDocumentStore documentStore,
            IImageFactory imageFactory,
            IConfigurationManager configurationManager)
        {
            _documentStore = documentStore;
            _imageFactory = imageFactory;
            _configurationManager = configurationManager;
            
            _imagesToImport = new List<Image>();
            _imagesToRetry = new List<string>();
        }

        public void Handle(ApplicationDataSyncRunEvent @event)
        {
            var s = _configurationManager.EmuPort();
            bool failed = false;
            try
            {
                log.Debug("Data sync begining");

                // Connect to Imu
                log.Debug("Connecting to Imu server: {0}:{1}", _configurationManager.EmuHost(), _configurationManager.EmuPort());
                _session = new Session(_configurationManager.EmuHost(), _configurationManager.EmuPort());
                _session.Connect();

                RunCatalogueDataSync(@event);
                RunMultimediaDataSync(@event);

                RetryLastFileImport();

                RunFileImport();
            }
            catch (Exception exception)
            {
                failed = true;
                log.Debug(exception.ToString);
            }

            // Possibly should be an event fired via repo.
            using (var documentSession = _documentStore.OpenSession())
            {
                var application = documentSession.Load<Application>(Constants.ApplicationId);

                if (application.DataImportCancelled || failed)
                {
                    log.Debug("Data sync finished (cancelled or failed)");
                    application.DataImportFinished();
                }
                else
                {
                    log.Debug("Data sync finished succesfully");
                    application.DataImportSuccess(@event.DateRun);
                }

                documentSession.SaveChanges();
            }
        }

        private void RunCatalogueDataSync(ApplicationDataSyncRunEvent @event)
        {
            // Get search ready
            var catalogue = new Module("ecatalogue", _session);
            var search = new Terms();
            search.Add("ColCategory", "History & Technology");
            search.Add("MdaDataSets_tab", "History & Technology Collections Online");
            search.Add("AdmPublishWebNoPassword", "Yes");
            search.Add("AdmDateModified", @event.LastDataImport.ToString("MMM dd yyyy"), ">=");

            // Perform catalogue search 
            log.Debug("Begining catalogue search for records modified since: {0}", @event.LastDataImport.ToString("MMM dd yyyy"));
            var stopwatch = Stopwatch.StartNew();
            var hits = catalogue.FindTerms(search);
            stopwatch.Stop();            
            log.Debug("Finished catalogue Emu search in {0:#,#} ms. {1} Hits", stopwatch.ElapsedMilliseconds, hits);

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
            var existingImageCount = 0;
            var newImageCount = 0;
            stopwatch = Stopwatch.StartNew();            

            // Create temporary images so we can compare with existing images without relying on the IMu API.
            var tempImages = new List<Image>();
            while (true)
            {
                using (var documentSession = _documentStore.OpenSession())
                {
                    if (documentSession.Load<Application>(Constants.ApplicationId).DataImportCancelled)
                    {
                        log.Debug("Cancel command recieved stopping Catalogue data sync");
                        return;
                    }

                    var results = catalogue.Fetch("start", count, Constants.DataBatchSize, columns);

                    if (results.Count == 0)
                        break;
                    
                    tempImages.AddRange(results.Rows.SelectMany(x => _imageFactory.MakeImages(x)));

                    count += results.Count;
                    log.Debug("Catalogue data fetch progress... {0}/{1}", count, hits);
                }
            }

            // Need to get distinct as we will end up with duplicates due to sharing of images.
            tempImages = tempImages.DistinctBy(x => x.Id).ToList();

            log.Debug("Comparing New images with existing ones");

            count = 0;

            // Using our temp images lets either update existing images or create new images.
            while (true)
            {
                using (var documentSession = _documentStore.OpenSession())
                {
                    if (documentSession.Load<Application>(Constants.ApplicationId).DataImportCancelled)
                    {
                        log.Debug("Cancel command recieved stopping Catalogue data sync");
                        return;
                    }

                    var tempImageBatch = tempImages
                        .Skip(count)
                        .Take(Constants.DataBatchSize)
                        .ToList();

                    if (!tempImageBatch.Any())
                        break;

                    var existingImages = documentSession.Load<Image>(tempImageBatch.Select(x => x.Id));

                    foreach (var tempImage in tempImageBatch)
                    {
                        var existingImage = existingImages.SingleOrDefault(x => x != null && x.Id == tempImage.Id);

                        if (existingImage != null) // If image exists in imagedocs, then update
                        {
                            existingImage.Update(
                                tempImage.CatalogueIrn,
                                tempImage.RegistrationNumber,
                                tempImage.Discipline,
                                tempImage.Category,
                                tempImage.Type,
                                tempImage.Title,
                                tempImage.Summary,
                                tempImage.PrimaryClassification,
                                tempImage.SecondaryClassification,
                                tempImage.TertiaryClassification,
                                tempImage.Significance,
                                tempImage.Description,
                                tempImage.ImageTitle,
                                tempImage.ImageDescription,
                                tempImage.Tags);
                            existingImageCount++;
                        }
                        else // else create new image
                        {
                            // Add to images so we can fetch the image later and persist then
                            _imagesToImport.Add(tempImage);
                            newImageCount++;
                        }
                    }

                    // Persist our document updates
                    documentSession.SaveChanges();
                    count += tempImageBatch.Count();

                    log.Debug("Catalogue data compare progress... {0}/{1}", count, tempImages.Count);
                }
            }

            stopwatch.Stop();
            log.Debug("Catalogue data sync finished, total Time: {0:0.00} Mins, updated images:{1}, new images:{2}", stopwatch.Elapsed.TotalMinutes, existingImageCount, newImageCount);
        }

        private void RunMultimediaDataSync(ApplicationDataSyncRunEvent @event)
        {
            // Get search ready
            var multimedia = new Module("emultimedia", _session);
            
            var search = new Terms();
            search.Add("AdmPublishWebNoPassword", "Yes");
            search.Add("AdmDateModified", @event.LastDataImport.ToString("MMM dd yyyy"), ">=");

            // Perform multimedia search 
            log.Debug("Begining multimedia search for records modified since: {0}", @event.LastDataImport.ToString("MMM dd yyyy"));
            var stopwatch = Stopwatch.StartNew();
            var hits = multimedia.FindTerms(search);
            stopwatch.Stop();
            log.Debug("Finished multimedia Emu search in {0:#,#} ms. {1} Hits", stopwatch.ElapsedMilliseconds, hits);

            var columns = new[] {
                                   "irn",
                                   "MulTitle",
                                   "MulDescription",
                                   "MulIdentifier",
                                   "AdmPublishWebNoPassword"
                               };

            var count = 0;
            var existingImageCount = 0;
            stopwatch = Stopwatch.StartNew();

            while (true)
            {
                using (var documentSession = _documentStore.OpenSession())
                {
                    if (documentSession.Load<Application>(Constants.ApplicationId).DataImportCancelled)
                    {
                        log.Debug("Cancel command recieved stopping multimedia data sync");
                        return;
                    }

                    var results = multimedia.Fetch("start", count, Constants.DataBatchSize, columns);

                    if (results.Count == 0)
                        break;

                    // Try Getting image documents so we can update existing.
                    // Dont need to worry about new multimedia records as they should be picked up by catalogue export.
                    var existingImages = documentSession.Load<Image>(results.Rows.Select(x => "Images/" + x.GetString("irn")));

                    foreach (var map in results.Rows)
                    {
                        var existingImage = existingImages.SingleOrDefault(x => x != null && x.Id == "Images/" + map.GetString("irn"));

                        // If image exists in imagedocs, then update, if not then it is not a HT multimedia record.
                        if (existingImage != null)
                        {
                            existingImage.Update(
                                map.GetString("MulTitle"),
                                map.GetString("MulDescription"));
                            existingImageCount++;

                            _imagesToImport.Add(existingImage);
                        }
                    }

                    count += results.Count;
                    log.Debug("Multimedia data sync progress... {0}/{1}", count, hits);
                }
            }

            stopwatch.Stop();
            log.Debug("Multimedia data sync finished, total Time: {0:0.00} Mins, updated images:{1}", stopwatch.Elapsed.TotalMinutes, existingImageCount);
        }

        private void RetryLastFileImport()
        {
            var count = 0;
            var maxRetryImageCount = 0;
            var removedImageCount = 0;
            var successImageCount = 0;
            var stopwatch = Stopwatch.StartNew();

            log.Debug("Fetching images that failed last import");

            // Get all our Images that need to be retried.  Cannot grab whole object as we may need to delete later and session will be stale by then so only grab Id.
            // Need to pre-fetch all our images as we are modifiying the field used to filter the query and we dont want to continually retry files that soft-fail.
            while (true)
            {
                using (var documentSession = _documentStore.OpenSession())
                {
                    var imageBatch = documentSession
                        .Query<Image, Images_ToBeRetried>()
                        .Customize(x => x.WaitForNonStaleResultsAsOfNow(TimeSpan.FromSeconds(Constants.WaitForNonStaleResultsTimeout)))
                        .Skip(count)
                        .Take(Constants.DataBatchSize)
                        .Select(x => x.Id)
                        .ToArray();

                    if (!imageBatch.Any())
                        break;

                    _imagesToRetry.AddRange(imageBatch);
                    count += imageBatch.Count();
                }
            }

            log.Debug("Retrying file import on images that failed last import, images to retry:{0}", _imagesToRetry.Count);

            count = 0;
            while (true)
            {
                using (var documentSession = _documentStore.OpenSession())
                {
                    if (documentSession.Load<Application>(Constants.ApplicationId).DataImportCancelled)
                    {
                        log.Debug("Cancel command recieved stopping File import");
                        return;
                    }

                    var imageBatch = _imagesToRetry
                        .Skip(count)
                        .Take(Constants.FileBatchSize)
                        .ToList();

                    if (!imageBatch.Any())
                        break;

                    // Perform fetch image on images
                    foreach (var image in documentSession.Load<Image>(imageBatch).Where(x => x != null))
                    {
                        if (image.ImageFetchAttempts < Constants.MaxImageFetchAttempts)
                        {
                            if (_imageFactory.TryFetchImage(_session, image))
                            {
                                successImageCount++;
                            }
                            else
                            {
                                documentSession.Delete(image);
                                removedImageCount++;
                            }
                        }
                        else
                        {
                            documentSession.Delete(image);
                            maxRetryImageCount++;
                        }
                    }

                    documentSession.SaveChanges();
                    count += imageBatch.Count();
                    log.Debug("Retry File Import progress... {0}/{1}", count, _imagesToRetry.Count);
                }
            }

            stopwatch.Stop();
            log.Debug("Retry File import finished, total Time: {0:0.00} Mins, success:{1}, removed due to errors:{2}, max retries reached:{3}", stopwatch.Elapsed.TotalMinutes, successImageCount, removedImageCount, maxRetryImageCount);
        }

        private void RunFileImport()
        {
            var count = 0;
            var removedImageCount = 0;
            var successImageCount = 0;
            var stopwatch = Stopwatch.StartNew();

            log.Debug("Begining File import on {0} images", _imagesToImport.Count);

            while (true)
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
                    if (!imageBatch.Any())
                        break;

                    // Perform fetch image on images
                    foreach (var image in imageBatch)
                    {
                        if (_imageFactory.TryFetchImage(_session, image))
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
                    count += imageBatch.Count();
                    log.Debug("File import progress... {0}/{1}", count, _imagesToImport.Count);
                }
            }

            stopwatch.Stop();
            log.Debug("File import finished, total Time: {0:0.00} Mins, success:{1}, removed due to errors:{2}", stopwatch.Elapsed.TotalMinutes, successImageCount, removedImageCount);
        }
    }
}