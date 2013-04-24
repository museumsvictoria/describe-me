using System.ComponentModel.DataAnnotations;

namespace DescribeMe.Web.ViewModels
{
    public class AccountExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ReturnUrl { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ExternalLoginData { get; set; }
    }
}