using DescribeMe.Core.Commands;
using DescribeMe.Core.Config;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.CommandHandlers
{
    public class ApplicationRunDataImportCommandHandler : ICommandHandler<ApplicationRunDataImportCommand>
    {
        private readonly IRepository<Application> _repository;

        public ApplicationRunDataImportCommandHandler(IRepository<Application> repository)
        {
            _repository = repository;
        }

        public void Handle(ApplicationRunDataImportCommand command)
        {
            // Load
            var application = _repository.GetById(Constants.ApplicationId);

            // Modify
            application.RunDataImport(command.DateRun);
            
            // Persist
            _repository.Save(application);
        }
    }
}