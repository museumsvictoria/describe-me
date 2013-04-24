using System.Linq;
using DescribeMe.Core.DomainModels;
using Raven.Client.Indexes;

namespace DescribeMe.Core.Indexes
{
    public class Images_DescribedByCount : AbstractIndexCreationTask<Image, Images_DescribedByCount.ReduceResult>
    {
        public class ReduceResult
        {
            public string UserName { get; set; }
            public int Count { get; set; }
        }

        public Images_DescribedByCount()
        {
            Map = images => from image in images
                            select new
                            {
                                UserName = image.DescribedByUser.Name,
                                Count = 1
                            };

            Reduce = results => from result in results
                                group result by result.UserName
                                into g
                                select new
                                {
                                    Count = g.Sum(x => x.Count),
                                    UserName = g.Key
                                };
        }
    }
}