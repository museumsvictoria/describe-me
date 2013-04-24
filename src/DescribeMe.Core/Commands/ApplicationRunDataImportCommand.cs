using System;
using DescribeMe.Core.Infrastructure;

namespace DescribeMe.Core.Commands
{
    public class ApplicationRunDataImportCommand : ICommand
    {
        public DateTime DateRun { get; set; }
    }
}