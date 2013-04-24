using DescribeMe.Core.Commands;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.CommandHandlers
{
    public class UserUpdateCommandHandler : ICommandHandler<UserUpdateCommand>
    {
        private readonly IRepository<User> _repository;

        public UserUpdateCommandHandler(IRepository<User> repository)
        {
            _repository = repository;
        }

        public void Handle(UserUpdateCommand command)
        {
            // Load
            var user = _repository.GetById(command.Id);

            // Modify
            user.Update(command.Name);
            
            // Persist
            _repository.SaveAsync(user);
        }
    }
}