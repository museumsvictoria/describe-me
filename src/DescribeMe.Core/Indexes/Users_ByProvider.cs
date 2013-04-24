using System.Linq;
using DescribeMe.Core.DomainModels;
using Raven.Client.Indexes;

namespace DescribeMe.Core.Indexes
{
    public class Users_ByProvider : AbstractIndexCreationTask<User>
    {
        public Users_ByProvider()
        {
            Map = users => from user in users
                            select new
                            {
                                ProviderUserId = user.ProviderUserId,
                                Provider = user.Provider
                            };
        }
    }
}