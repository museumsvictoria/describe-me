using DescribeMe.Core.Config;

namespace DescribeMe.Core.Factories
{
    public class ImagePathFactory : IImagePathFactory
    {
        private readonly IConfigurationManager _configurationManager;

        public ImagePathFactory(
            IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public string MakeUriPath(string id)
        {
            return string.Format("/images/{0}/{1}.jpg", GetSubFolder(id), id);
        }

        public string MakeDestUncPath(string id)
        {
            return string.Format("{0}\\{1}\\{2}.jpg", _configurationManager.ImagesPath(), GetSubFolder(id), id);
        }

        /// <summary>
        /// Used to generate a sub folder for images to ensure we dont one day run into the rough limit of 300,000 files per directory.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int GetSubFolder(string id)
        {            
            return int.Parse(id)%10;
        }
    }
}