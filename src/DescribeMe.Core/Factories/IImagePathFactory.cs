namespace DescribeMe.Core.Factories
{
    public interface IImagePathFactory
    {
        string MakeUriPath(string id);

        string MakeDestUncPath(string id);
    }
}