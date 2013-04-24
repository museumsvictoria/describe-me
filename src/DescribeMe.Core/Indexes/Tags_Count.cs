using System.Linq;
using DescribeMe.Core.DomainModels;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace DescribeMe.Core.Indexes
{
    public class Tags_Count : AbstractIndexCreationTask<Image, Tags_Count.ReduceResult>
    {
        public class ReduceResult
        {
            public string Tag { get; set; }
            public int Count { get; set; }
        }

        public Tags_Count()
        {
            Map = images => from image in images
                            where image.Filename != null && image.UserAltDescription == null
                            from tag in image.Tags
                            select new
                            {
                                Tag = tag,
                                Count = 1
                            };

            Reduce = results => from result in results
                                group result by result.Tag
                                into g
                                select new
                                {
                                    Tag = g.Key,
                                    Count = g.Sum(x => x.Count)
                                };

            Index(x => x.Tag, FieldIndexing.Analyzed);
        }
    }
}