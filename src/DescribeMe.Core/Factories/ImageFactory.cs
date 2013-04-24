using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Services;
using IMu;
using NLog;

namespace DescribeMe.Core.Factories
{
    public class ImageFactory : IImageFactory
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IImageService _imageService;
        private readonly IImagePathFactory _imagePathFactory;
        private readonly ISlugFactory _slugFactory;

        public ImageFactory(
            IImageService imageService,
            IImagePathFactory imagePathFactory,
            ISlugFactory slugFactory)
        {
            _imageService = imageService;
            _imagePathFactory = imagePathFactory;
            _slugFactory = slugFactory;
        }

        public IEnumerable<Image> MakeImages(Map map)
        {
            var images = new List<Image>();

            // Catalogue
            var catalogueIrn = map.GetString("irn");

            string registrationNumber;
            if (map["ColRegPart"] != null)
            {
                registrationNumber = string.Format("{0}{1}.{2}", map["ColRegPrefix"], map["ColRegNumber"], map["ColRegPart"]);
            }
            else
            {
                registrationNumber = string.Format("{0}{1}", map["ColRegPrefix"], map["ColRegNumber"]);
            }

            string discipline = map.GetString("ColDiscipline");
            string category = map.GetString("ColCategory");
            string type = map.GetString("ColTypeOfItem");

            string title = map.GetString("ClaObjectName");
            string summary = map.GetString("ClaObjectSummary");
            string primaryClassification = map.GetString("ClaPrimaryClassification");
            string secondaryClassification = map.GetString("ClaSecondaryClassification");
            string tertiaryClassification = map.GetString("ClaTertiaryClassification");
            string significance = map.GetString("SubHistoryTechSignificance");
            string description = map.GetString(!string.IsNullOrWhiteSpace((string)map["Con1Description"]) ? "Con1Description" : "DesPhysicalDescription");

            var tags = map.GetStrings("SubSubjects_tab").Select(x => _slugFactory.MakeSlug(x)).ToArray();

            // Multimedia, skip MM records that already contain an image description, and numismatics records.
            if ((type != "Image" ||
                (type == "Image" &&
                string.IsNullOrWhiteSpace((string)map["Con1Description"]) &&
                map.GetMaps("images").Any(x => string.IsNullOrWhiteSpace(x.GetString("MulDescription")) && x.GetString("AdmPublishWebNoPassword") == "Yes"))) &&
                discipline != "Numismatics")
            {
                // Determine what images we are adding.  
                // First filter images, then create images
                images = map.GetMaps("images")
                    .Where(x => x != null && x.GetString("MulIdentifier") != null &&
                                (x.GetString("MulIdentifier").EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                 x.GetString("MulIdentifier").EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                 x.GetString("MulIdentifier").EndsWith(".tif", StringComparison.OrdinalIgnoreCase) ||
                                 x.GetString("MulIdentifier").EndsWith(".tiff", StringComparison.OrdinalIgnoreCase))
                                && !x.GetString("MulIdentifier").Contains("Graphic Warning.tiff")
                                && x.GetString("AdmPublishWebNoPassword") != "No")
                    .Select(x => new Image(
                                     x.GetString("irn"),
                                     catalogueIrn,
                                     registrationNumber,
                                     discipline,
                                     category,
                                     type,
                                     title,
                                     summary,
                                     primaryClassification,
                                     secondaryClassification,
                                     tertiaryClassification,
                                     significance,
                                     description,
                                     x.GetString("MulTitle"),
                                     x.GetString("MulDescription"),
                                     tags))
                    .ToList();
            }

            return images;
        }

        /// <summary>
        /// Attempts to fetch an image and save the result to disk.  
        /// </summary>
        /// <param name="session">IMu Session object</param>
        /// <param name="image">Image to be fetched</param>
        /// <param name="overwriteExistingImage"></param>
        /// <returns>Boolean indicating whether the image was successfully saved or marked for another attempt later OR the image cannot be saved at all.</returns>
        public bool TryFetchImage(Session session, Image image, bool overwriteExistingImage = true)
        {
            var destPath = _imagePathFactory.MakeDestUncPath(image.ImageIrn);

            // Only overwrite image if explicitly requested OR the file doesnt exist.
            if (overwriteExistingImage || !File.Exists(destPath))
            {
                try
                {
                    var multimedia = new Module("emultimedia", session);
                    multimedia.FindKey(long.Parse(image.ImageIrn));
                    var result = multimedia.Fetch("start", 0, -1, new[] {"resource"}).Rows[0];

                    var fileStream = result.GetMap("resource")["file"] as FileStream;

                    // Save file
                    _imageService.Save(fileStream, image.ImageIrn);

                    // Set filename after successful saving of image
                    image.SetFilename(_imagePathFactory.MakeUriPath(image.ImageIrn));
                }
                catch (Exception exception)
                {
                    // Only remove the image from the export if it is an error we cannot retry.
                    if (exception is IMuException && ((IMuException) exception).ID == "MultimediaResolutionNotFound")
                    {
                        log.Debug("Error saving image {0}, un-recoverable error, image removed from export {1}", image.Id, exception.ToString());
                        return false;
                    }

                    // Mark image so we can retry fetch later
                    image.IncrementImageFetchAttempts();

                    log.Debug("Error saving image {0}, will be retried when data export re-run {1}", image.Id, exception.ToString());
                }
            }
            else
            {
                // Ensure we set a filename if file exists and we are not overwriting it.
                image.SetFilename(_imagePathFactory.MakeUriPath(image.ImageIrn));
            }

            return true;
        }
    }
}