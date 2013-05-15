using System.ComponentModel.DataAnnotations;
using DescribeMe.Core.Validators;

namespace DescribeMe.Web.ViewModels
{
    public class AccountExternalLoginConfirmationInput
    {
        [Required]
        [Display(Name = "User name")]
        [UniqueUserName(ErrorMessage = "User name already exists. Please enter a different user name.")]
        public string UserName { get; set; }

        public string ReturnUrl { get; set; }
        
        public string ExternalLoginData { get; set; }
    }
}