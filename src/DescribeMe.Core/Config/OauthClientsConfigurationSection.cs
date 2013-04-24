using System.Configuration;

namespace DescribeMe.Core.Config
{
    public class OauthClientsConfigurationSection : ConfigurationSection
    {
        [ConfigurationCollection(typeof(OauthClientsCollection), AddItemName = "client")]
        [ConfigurationProperty("", IsDefaultCollection = true)]        
        public OauthClientsCollection OauthClients
        {
            get { return this[""] as OauthClientsCollection; }
        }
    }
    
    public class OauthClientsCollection : ConfigurationElementCollection
    {
        public ClientConfigurationElement this[object key]
        {
            get { return base.BaseGet(key) as ClientConfigurationElement; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ClientConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ClientConfigurationElement)(element)).Name;
        }
    }
}