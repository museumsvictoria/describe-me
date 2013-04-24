using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.Commands
{
    public class ImageApproveCommand : ICommand
    {
        public string Id { get; set; }

        public string UserAltDescription { get; set; }
    }
}