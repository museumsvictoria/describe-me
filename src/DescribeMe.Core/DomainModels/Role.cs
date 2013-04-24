namespace DescribeMe.Core.DomainModels
{
    public class Role : DomainModel, INamedDomainModel
    {
        public string Name { get; private set; }

        public string Description { get; private set; }

        public Role(
            string id,
            string name,
            string description)
        {
            Id = "roles/" + id;
            Name = name;
            Description = description;
        }
    }
}