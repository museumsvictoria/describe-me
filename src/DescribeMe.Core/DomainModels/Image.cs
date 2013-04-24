using System.Collections.Generic;
using DescribeMe.Core.DesignByContract;
using DescribeMe.Core.Events;

namespace DescribeMe.Core.DomainModels
{
    public class Image : DomainModel
    {
        public string ImageIrn { get; private set; }

        public string CatalogueIrn { get; private set; }

        public string RegistrationNumber { get; private set; }

        public string Discipline { get; private set; }

        public string Category { get; private set; }

        public string Type { get; private set; }

        public string Title { get; private set; }

        public string Summary { get; private set; }

        public string PrimaryClassification { get; private set; }

        public string SecondaryClassification { get; private set; }

        public string TertiaryClassification { get; private set; }

        public string Significance { get; private set; }

        public string Description { get; private set; }

        public string ImageTitle { get; private set; }

        public string ImageDescription { get; private set; }

        public string Filename { get; private set; }

        public string UserAltDescription { get; private set; }

        public DenormalizedReference<User> DescribedByUser { get; private set; }

        public bool Approved { get; private set; }

        public int ImageFetchAttempts { get; private set; }

        public ICollection<string> Tags { get; private set; }

        public Image(
            string imageIrn, 
            string catalogueIrn, 
            string registrationNumber,
            string discipline,
            string category,
            string type,
            string title,
            string summary,
            string primaryClassification,
            string secondaryClassification,
            string tertiaryClassification,
            string significance,
            string description,
            string imageTitle,
            string imageDescription,
            ICollection<string> tags)
        {
            Id = "images/" + imageIrn;
            ImageIrn = imageIrn;
            CatalogueIrn = catalogueIrn;
            RegistrationNumber = registrationNumber;
            Discipline = discipline;
            Category = category;
            Type = type;
            Title = title;
            Summary = summary;
            PrimaryClassification = primaryClassification;
            SecondaryClassification = secondaryClassification;
            TertiaryClassification = tertiaryClassification;
            Significance = significance;
            Description = description;
            ImageTitle = imageTitle;
            ImageDescription = imageDescription;
            Tags = tags;
        }

        public void SetFilename(string filename)
        {
            Requires.IsNotNullOrWhitespace(filename, "filename");

            Filename = filename;
        }

        public void DescribeImage(string userAltDescription, User user)
        {
            Requires.IsNotNullOrWhitespace(userAltDescription, "userAltDescription");

            UserAltDescription = userAltDescription;
            DescribedByUser = user;

            ApplyEvent(new ImageCountUpdatedEvent(this));
        }

        public void ApproveImage(string userAltDescription)
        {
            Requires.IsNotNullOrWhitespace(userAltDescription, "userAltDescription");

            UserAltDescription = userAltDescription;
            Approved = true;

            ApplyEvent(new ImageCountUpdatedEvent(this));
        }

        public void RejectImage()
        {
            UserAltDescription = null;
            DescribedByUser = null;

            ApplyEvent(new ImageCountUpdatedEvent(this));
        }

        public void IncrementImageFetchAttempts()
        {
            ImageFetchAttempts++;
        }

        public void Update(
            string catalogueIrn, 
            string registrationNumber,
            string discipline,
            string category,
            string type,
            string title,
            string summary,
            string primaryClassification,
            string secondaryClassification,
            string tertiaryClassification,
            string significance,
            string description,
            string imageTitle,
            string imageDescription,
            ICollection<string> tags)
        {
            CatalogueIrn = catalogueIrn;
            RegistrationNumber = registrationNumber;
            Discipline = discipline;
            Category = category;
            Type = type;
            Title = title;
            Summary = summary;
            PrimaryClassification = primaryClassification;
            SecondaryClassification = secondaryClassification;
            TertiaryClassification = tertiaryClassification;
            Significance = significance;
            Description = description;
            ImageTitle = imageTitle;
            ImageDescription = imageDescription;
            Tags = tags;
        }

        public void Update(
            string imageTitle,
            string imageDescription)
        {
            ImageTitle = imageTitle;
            ImageDescription = imageDescription;
        }
    }
}