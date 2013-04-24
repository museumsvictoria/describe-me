using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.Commands
{
    public class ImageRejectCommand : ICommand
    {
        public string Id { get; set; }
    }
}