using System.Configuration;

namespace DescribeMe.Core.Config
{
    public class ModeratorsConfigurationSection : ConfigurationSection
    {
        [ConfigurationCollection(typeof(OauthClientsCollection), AddItemName = "user")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public ModeratorsCollection Moderators
        {
            get { return this[""] as ModeratorsCollection; }
        }
    }

    public class ModeratorsCollection : ConfigurationElementCollection
    {
        public UserConfigurationElement this[object key]
        {
            get { return base.BaseGet(key) as UserConfigurationElement; }
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