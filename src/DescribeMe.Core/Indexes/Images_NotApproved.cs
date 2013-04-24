using System.Linq;
using DescribeMe.Core.DomainModels;
using Raven.Client.Indexes;

namespace DescribeMe.Core.Indexes
{
    public class Images_NotApproved : AbstractIndexCreationTask<Image>
    {
        public Images_NotApproved()
        {
            Map = images => from image in images
                            where image.UserAltDescription != null && !image.Approved
                            select new
                            {
                                Tags = image.Tags
                            };
        }
    }
}