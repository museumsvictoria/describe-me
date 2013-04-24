using System.Configuration;

namespace DescribeMe.Core.Config
{
    public class EnvironmentConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("imagesPath", DefaultValue = "", IsRequired = true, IsKey = false)]
        public string ImagesPath
        {
            get { return (string)this["imagesPath"]; }
            set { this["imagesPath"] = value; }
        }

        [ConfigurationProperty("siteUrl", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string SiteUrl
        {
            get { return (string)this["siteUrl"]; }
            set { this["siteUrl"] = value; }
        }
    }
}
