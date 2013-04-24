using System.Configuration;

namespace DescribeMe.Core.Config
{
    public class UserConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("userId", IsRequired = true)]
        public string UserId
        {
            get { return this["userId"] as string; }
            set { this["userId"] = value; }
        }

        [ConfigurationProperty("provider", IsRequired = true)]
        public string Provider
        {
            get { return this["provider"] as string; }
            set { this["provider"] = value; }
        }

        [ConfigurationProperty("name", IsRequired = false)]
        public string Name
        {
            get { return this["name"] as string; }
            set { this["name"] = value; }
        }
    }
}