using System.Linq;
using DescribeMe.Core.DomainModels;
using Raven.Client.Indexes;

namespace DescribeMe.Core.Indexes
{
    public class Images_DescribedByUserId : AbstractIndexCreationTask<Image>
    {
        public Images_DescribedByUserId()
        {
            Map = images => from image in images
                            where image.DescribedByUser != null
                            select new
                            {
                                Id = image.DescribedByUser.Id
                            };
        }
    }
}