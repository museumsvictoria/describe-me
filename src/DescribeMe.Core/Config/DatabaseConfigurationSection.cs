using System.Configuration;

namespace DescribeMe.Core.Config
{
    public class DatabaseConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("url", DefaultValue = "", IsRequired = true, IsKey = false)]
        public string Url
        {
            get { return (string)this["url"]; }
            set { this["url"] = value; }
        }

        [ConfigurationProperty("name", DefaultValue = "", IsRequired = true, IsKey = false)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }
    }
}
