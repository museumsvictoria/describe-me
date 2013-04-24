using System;

namespace DescribeMe.Web.ViewModels
{
    public class StatisticsViewModel
    {
        public int DescribedImageCount { get; set; }
        public int UnDescribedImageCount { get; set; }
        public int ApprovedImageCount { get; set; }
        public int UnApprovedImageCount { get; set; }
        
        public int TotalImageCount
        {
            get { return DescribedImageCount + UnDescribedImageCount; }
        }

        public int TotalReviewImageCount
        {
            get { return ApprovedImageCount + UnApprovedImageCount; }
        }

        public decimal DescribedImageCountAsPercentage
        {
            get
            {
                if (TotalImageCount > 0)
                {
                    var percentage = (decimal)DescribedImageCount / TotalImageCount * 100;

                    return Math.Round(percentage, 4);
                }

                return 0;
            }
        }

        public decimal ApprovedImageCountAsPercentage
        {
            get
            {
                if (TotalReviewImageCount > 0)
                {
                    var percentage = (decimal)ApprovedImageCount / TotalReviewImageCount * 100;

                    return Math.Round(percentage, 4);
                }

                return 0;
            }
        }
    }
}