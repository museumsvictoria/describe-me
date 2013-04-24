using DescribeMe.Core.Commands;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.CommandHandlers
{
    public class ImageDescribeCommandHandler : ICommandHandler<ImageDescribeCommand>
    {
        private readonly IRepository<Image> _repository;

        public ImageDescribeCommandHandler(IRepository<Image> repository)
        {
            _repository = repository;
        }

        public void Handle(ImageDescribeCommand command)
        {
            // Load
            var image = _repository.GetById(command.Id);

            // Modify
            image.DescribeImage(command.UserAltDescription, command.User);
            
            // Persist
            _repository.SaveAsync(image);
        }
    }
}