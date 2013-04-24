using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.Commands
{
    public class UserUpdateCommand : ICommand
    {
        public string Name { get; set; }

        public string Id { get; set; }
    }
}