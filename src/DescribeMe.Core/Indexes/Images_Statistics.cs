using System.Linq;
using DescribeMe.Core.DomainModels;
using Raven.Client.Indexes;

namespace DescribeMe.Core.Indexes
{
    public class Images_Statistics : AbstractIndexCreationTask<Image, Images_Statistics.ReduceResult>
    {
        public class ReduceResult
        {
            public int DescibedImageCount { get; set; }
            public int UnDescibedImageCount { get; set; }
            public int ApprovedImageCount { get; set; }
            public int UnApprovedImageCount { get; set; }
        }

        public Images_Statistics()
        {
            Map = images => from image in images
                            select new
                            {
                                DescibedImageCount = (image.UserAltDescription != null) ? 1 : 0,
                                UnDescibedImageCount = (image.UserAltDescription == null) ? 1 : 0,
                                ApprovedImageCount = (image.Approved && image.UserAltDescription != null) ? 1 : 0,
                                UnApprovedImageCount = (!image.Approved && image.UserAltDescription != null) ? 1 : 0
                            };

            Reduce = results => from result in results
                                group result by "constant"
                                into g
                                select new
                                {
                                    DescibedImageCount = g.Sum(x => x.DescibedImageCount),
                                    UnDescibedImageCount = g.Sum(x => x.UnDescibedImageCount),
                                    ApprovedImageCount = g.Sum(x => x.ApprovedImageCount),
                                    UnApprovedImageCount = g.Sum(x => x.UnApprovedImageCount)
                                };
        }
    }
}