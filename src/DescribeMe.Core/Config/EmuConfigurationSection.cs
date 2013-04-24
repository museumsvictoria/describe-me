using System.Configuration;

namespace DescribeMe.Core.Config
{
    public class EmuConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("host", DefaultValue = "", IsRequired = true, IsKey = false)]
        public string Host
        {
            get { return (string)this["host"]; }
            set { this["host"] = value; }
        }

        [ConfigurationProperty("port", IsRequired = true, IsKey = false)]
        public int Port
        {
            get { return (int)this["port"]; }
            set { this["port"] = value; }
        }
    }
}
