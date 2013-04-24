using System.Configuration;

namespace DescribeMe.Core.Config
{
    public class AdminsConfigurationSection : ConfigurationSection
    {
        [ConfigurationCollection(typeof(UserConfigurationElement), AddItemName = "user")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public AdminsCollection Admins
        {
            get
            {
                return this[""] as AdminsCollection;
            }
        }
    }
    
    public class AdminsCollection : ConfigurationElementCollection
    {
        public UserConfigurationElement this[object key]
        {
            get
            {
                return base.BaseGet(key) as UserConfigurationElement;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new UserConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((UserConfigurationElement)(element)).Name;
        }
    }
}