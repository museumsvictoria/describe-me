using System.ComponentModel.DataAnnotations;

namespace DescribeMe.Web.ViewModels
{
    public class AccountManageViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }
}