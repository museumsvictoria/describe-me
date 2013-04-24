using System.Collections.Generic;
using DescribeMe.Core.DomainModels;

namespace DescribeMe.Core.Config
{
    public class ConfigurationManager : IConfigurationManager
    {
        public string DatabaseUrl()
        {
            return ((DatabaseConfigurationSection)System.Configuration.ConfigurationManager.GetSection("describeme/database")).Url;
        }

        public string DatabaseName()
        {
            return ((DatabaseConfigurationSection)System.Configuration.ConfigurationManager.GetSection("describeme/database")).Name;
        }

        public string EmuHost()
        {
            return ((EmuConfigurationSection)System.Configuration.ConfigurationManager.GetSection("describeme/emu")).Host;
        }

        public int EmuPort()
        {
            return ((EmuConfigurationSection)System.Configuration.ConfigurationManager.GetSection("describeme/emu")).Port;
        }

        public string ImagesPath()
        {
            return ((EnvironmentConfigurationSection)System.Configuration.ConfigurationManager.GetSection("describeme/environment")).ImagesPath;
        }

        public string SiteUrl()
        {
            return ((EnvironmentConfigurationSection)System.Configuration.ConfigurationManager.GetSection("describeme/environment")).SiteUrl;
        }

        public string OauthClientKey(string name)
        {
            var oauthClients = (OauthClientsConfigurationSection)System.Configuration.ConfigurationManager.GetSection("describeme/oauthClients");

            return oauthClients.OauthClients[name].Key;
        }

        public string OauthClientSecret(string name)
        {
            var oauthClients = (OauthClientsConfigurationSection)System.Configuration.ConfigurationManager.GetSection("describeme/oauthClients");

            return oauthClients.OauthClients[name].Secret;
        }

        public ICollection<User> GetModerators()
        {
            var moderators = new List<User>();

            foreach (UserConfigurationElement moderator in ((ModeratorsConfigurationSection)System.Configuration.ConfigurationManager.GetSection("describeme/moderators")).Moderators)
            {
                moderators.Add(new User(moderator.UserId, moderator.Provider, moderator.Name));
            }

            return moderators;
        }

        public ICollection<User> GetAdmins()
        {
            var admins = new List<User>();

            foreach (UserConfigurationElement moderator in ((AdminsConfigurationSection)System.Configuration.ConfigurationManager.GetSection("describeme/admins")).Admins)
            {
                admins.Add(new User(moderator.UserId, moderator.Provider, moderator.Name));
            }

            return admins;
        }
    }
}