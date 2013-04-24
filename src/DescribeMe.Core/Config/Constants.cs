namespace DescribeMe.Core.Config
{
    public static class Constants
    {
        public static string ApplicationId = "applications/describeme";

        public static int DataBatchSize = 500;

        public static int FileBatchSize = 10;

        public static int MaxImageFetchAttempts = 5;

        public static int WaitForNonStaleResultsTimeout = 15;

        public static int MaxLinesConsoleLogTail = 50;
    }
}
