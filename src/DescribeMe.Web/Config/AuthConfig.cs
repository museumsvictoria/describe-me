using DescribeMe.Core.Config;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Web.WebPages.OAuth;

namespace DescribeMe.Web.Config
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            var configurationManager = ServiceLocator.Current.GetInstance<IConfigurationManager>();

            OAuthWebSecurity.RegisterTwitterClient(
                consumerKey: configurationManager.OauthClientKey("twitter"),
                consumerSecret: configurationManager.OauthClientSecret("twitter"));
        }
    }
}