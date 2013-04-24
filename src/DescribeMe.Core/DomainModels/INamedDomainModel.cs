namespace DescribeMe.Core.DomainModels
{
    public interface INamedDomainModel
    {
        string Id { get; }
        string Name { get; }
    }
}