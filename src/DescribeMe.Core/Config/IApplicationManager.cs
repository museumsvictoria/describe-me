namespace DescribeMe.Core.Config
{
    public interface IApplicationManager
    {
        ApplicationManager SetupApplication();

        ApplicationManager RegisterRavenWebsiteChanges();
    }
}
