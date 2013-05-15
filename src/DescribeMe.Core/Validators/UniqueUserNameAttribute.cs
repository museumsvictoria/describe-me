using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DescribeMe.Core.Config;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Indexes;
using Ninject;
using Raven.Client;
using Raven.Client.Linq;

namespace DescribeMe.Core.Validators
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueUserNameAttribute : ValidationAttribute
    {
        [Inject]
        public IDocumentSession DocumentSession { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (DocumentSession.Query<User, Users_ByName>().Any(x => x.Name.Equals((string)value, StringComparison.CurrentCultureIgnoreCase)))
            {
                return new ValidationResult(ErrorMessageString);
            }

            return ValidationResult.Success;
        }
    }
}