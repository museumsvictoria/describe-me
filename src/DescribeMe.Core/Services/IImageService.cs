using System.IO;

namespace DescribeMe.Core.Services
{
    public interface IImageService
    {
        void Save(FileStream file, string id);
    }
}