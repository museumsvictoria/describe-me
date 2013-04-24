using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.Commands
{
    public class ImageDescribeCommand : ICommand
    {
        public string Id { get; set; }

        public string UserAltDescription { get; set; }

        public User User { get; set; }
    }
}