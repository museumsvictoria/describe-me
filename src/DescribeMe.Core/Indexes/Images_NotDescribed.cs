using System.Linq;
using DescribeMe.Core.DomainModels;
using Raven.Client.Indexes;

namespace DescribeMe.Core.Indexes
{
    public class Images_NotDescribed : AbstractIndexCreationTask<Image>
    {
        public Images_NotDescribed()
        {
            Map = images => from image in images
                            where image.Filename != null && image.UserAltDescription == null
                            select new
                            {
                                Tags = image.Tags
                            };
        }
    }
}