using System.Linq;
using DescribeMe.Core.DomainModels;
using Raven.Client.Indexes;

namespace DescribeMe.Core.Indexes
{
    public class Images_ToBeRetried : AbstractIndexCreationTask<Image>
    {
        public Images_ToBeRetried()
        {
            Map = images => from image in images
                            where image.Filename == null
                            select new
                            {
                                Id = image.Id
                            };
        }
    }
}