using DescribeMe.Core.DomainModels;

namespace DescribeMe.Core.Infrastructure
{
    public interface IRepository<T> where T : DomainModel
    {
        void Save(DomainModel domainModel);

        void SaveAsync(DomainModel domainModel);

        T GetById(string id);
    }
}