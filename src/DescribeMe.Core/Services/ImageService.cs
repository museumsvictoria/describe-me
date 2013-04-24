using System.IO;
using DescribeMe.Core.Factories;
using ImageResizer;

namespace DescribeMe.Core.Services
{
    public class ImageService : IImageService
    {
        private readonly IImagePathFactory _imagePathFactory;

        public ImageService(IImagePathFactory imagePathFactory)
        {
            _imagePathFactory = imagePathFactory;
        }

        public void Save(FileStream file, string id)
        {
            // Find the path
            var path = _imagePathFactory.MakeDestUncPath(id);

            // Create directory
            var pathDir = path.Remove(path.LastIndexOf('\\') + 1);
            if (!Directory.Exists(pathDir))
            {
                Directory.CreateDirectory(pathDir);
            }

            // Delete file if it exists as we want to ensure it is overwritten
            if(File.Exists(path))
            {
                File.Delete(path);
            }

            // Save Image to disk
            ImageBuilder.Current.Build(file, path, new ResizeSettings { Format = "jpg", Quality = 65, MaxHeight = 1200, MaxWidth = 1200 });
        }
    }
}