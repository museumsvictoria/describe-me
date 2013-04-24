using System.Configuration;

namespace DescribeMe.Core.Config
{
    public class ClientConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"] as string; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("key", IsRequired = true)]
        public string Key
        {
            get { return this["key"] as string; }
            set { this["key"] = value; }
        }

        [ConfigurationProperty("secret", IsRequired = false)]
        public string Secret
        {
            get { return this["secret"] as string; }
            set { this["secret"] = value; }
        }
    }
}