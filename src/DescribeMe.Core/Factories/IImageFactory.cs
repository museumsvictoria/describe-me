using System.Collections.Generic;
using DescribeMe.Core.DomainModels;
using IMu;

namespace DescribeMe.Core.Factories
{
    public interface IImageFactory
    {
        IEnumerable<Image> MakeImages(Map map);

        bool TryFetchImage(Session session, Image image, bool overwriteExistingImage = true);
    }
}