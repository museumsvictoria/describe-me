using System.IO;
using DescribeMe.Core.Factories;

namespace DescribeMe.Import.Factories
{
    public class ImagePathFactory : IImagePathFactory
    {
        public string MakeUriPath(string id)
        {
            return string.Format("/images/{0}/{1}.jpg", GetSubFolder(id), id);
        }

        public string MakeDestUncPath(string id)
        {
            return string.Format("{0}images\\{1}\\{2}.jpg", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), GetSubFolder(id), id);
        }

        /// <summary>
        /// Used to generate a sub folder for images to ensure we dont run into the rough limit of 300,000 files per directory.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int GetSubFolder(string id)
        {            
            return int.Parse(id)%10;
        }
    }
}