using System.Linq;
using DescribeMe.Core.DomainModels;
using Raven.Client.Indexes;

namespace DescribeMe.Core.Indexes
{
    public class Users_ByName : AbstractIndexCreationTask<User>
    {
        public Users_ByName()
        {
            Map = users => from user in users
                            select new
                            {
                                Name = user.Name
                            };
        }
    }
}