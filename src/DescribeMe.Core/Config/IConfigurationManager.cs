using System.Collections.Generic;
using DescribeMe.Core.DomainModels;

namespace DescribeMe.Core.Config
{
    public interface IConfigurationManager
    {
        string DatabaseUrl();

        string DatabaseName();

        string EmuHost();

        int EmuPort();

        string ImagesPath();

        string SiteUrl();

        string OauthClientKey(string name);

        string OauthClientSecret(string name);

        ICollection<User> GetModerators();

        ICollection<User> GetAdmins();
    }
}