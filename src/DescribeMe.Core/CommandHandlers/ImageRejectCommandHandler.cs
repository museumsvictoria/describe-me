using DescribeMe.Core.Commands;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.CommandHandlers
{
    public class ImageRejectCommandHandler : ICommandHandler<ImageRejectCommand>
    {
        private readonly IRepository<Image> _repository;

        public ImageRejectCommandHandler(IRepository<Image> repository)
        {
            _repository = repository;
        }

        public void Handle(ImageRejectCommand command)
        {
            // Load
            var image = _repository.GetById(command.Id);

            // Modify
            image.RejectImage();
            
            // Persist
            _repository.SaveAsync(image);
        }
    }
}