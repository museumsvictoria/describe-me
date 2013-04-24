using DescribeMe.Core.Commands;
using DescribeMe.Core.Config;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.CommandHandlers
{
    public class ApplicationCancelDataImportHandler : ICommandHandler<ApplicationCancelDataImportCommand>
    {
        private readonly IRepository<Application> _repository;

        public ApplicationCancelDataImportHandler(IRepository<Application> repository)
        {
            _repository = repository;
        }

        public void Handle(ApplicationCancelDataImportCommand command)
        {
            // Load
            var application = _repository.GetById(Constants.ApplicationId);

            // Modify
            application.CancelDataImport();

            // Persist
            _repository.SaveAsync(application);
        }
    }
}