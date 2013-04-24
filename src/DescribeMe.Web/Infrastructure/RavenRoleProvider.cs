using System;
using System.Web.Security;
using System.Linq;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Indexes;
using DescribeMe.Core.Utilities;
using Microsoft.Practices.ServiceLocation;
using Raven.Client;
using Raven.Client.Linq;

namespace DescribeMe.Web.Infrastructure
{
    public class RavenRoleProvider : RoleProvider
    {
        private readonly IDocumentStore _documentStore;

        public RavenRoleProvider()
        {
            _documentStore = ServiceLocator.Current.GetInstance<IDocumentStore>();
        }

        public override string[] GetRolesForUser(string username)
        {            
            using (var documentSession = _documentStore.OpenSession())
            {
                var user = documentSession
                    .Query<User, Users_ByName>()
                    .Where(x => x.Name == username)
                    .FirstOrDefault();

                if (user != null)
                    return user.Roles.Select(x => RavenIdResolver.ResolveToString(x.Id)).ToArray();
            }

            return new string[] { };
        }

        #region NotImplemented

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName { get; set; }

        #endregion

    }
}