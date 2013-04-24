using DescribeMe.Core.Commands;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.CommandHandlers
{
    public class ImageApproveCommandHandler : ICommandHandler<ImageApproveCommand>
    {
        private readonly IRepository<Image> _repository;

        public ImageApproveCommandHandler(IRepository<Image> repository)
        {
            _repository = repository;
        }

        public void Handle(ImageApproveCommand command)
        {
            // Load
            var image = _repository.GetById(command.Id);

            // Modify
            image.ApproveImage(command.UserAltDescription);
            
            // Persist
            _repository.SaveAsync(image);
        }
    }
}