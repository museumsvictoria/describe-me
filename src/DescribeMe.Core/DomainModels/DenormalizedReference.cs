namespace DescribeMe.Core.DomainModels
{
    public class DenormalizedReference<T> where T : INamedDomainModel
    {
        public string Id { get; private set; }
        public string Name { get; private set; }

        public static implicit operator DenormalizedReference<T> (T doc)
        {
            if (doc == null)
                return null;

            return new DenormalizedReference<T>
                   {
                       Id = doc.Id,
                       Name = doc.Name
                   };
        }
    }
}
