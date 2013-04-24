using System.Collections.Generic;
using System.Linq;
using DescribeMe.Core.DesignByContract;
using DescribeMe.Core.Events;

namespace DescribeMe.Core.DomainModels
{
    public class User : DomainModel, INamedDomainModel
    {
        public string ProviderUserId { get; private set; }

        public string Provider { get; private set; }

        public string Name { get; private set; }

        public ICollection<DenormalizedReference<Role>> Roles { get; private set; }

        public User(
            string providerUserId,
            string provider,
            string name)
        {
            Id = "users/";
            ProviderUserId = providerUserId;
            Provider = provider;
            Name = name;

            InitMembers();
        }

        private void InitMembers()
        {
            Roles = new List<DenormalizedReference<Role>>();
        }

        public void AddRoles(IEnumerable<Role> roles)
        {
            Requires.IsNotNull(roles, "roles");

            foreach (var role in roles)
            {               
                if (Roles.All(x => x.Id != role.Id))
                {
                    Roles.Add(role);
                }
            }
        }

        public void AddRole(Role role)
        {
            Requires.IsNotNull(role, "roles");

            if (Roles.All(x => x.Id != role.Id))
            {
                Roles.Add(role);
            }
        }

        public void Update(
            string name)
        {
            Name = name;

            ApplyEvent(new UserUpdatedEvent(this, name));
        }
    }
}