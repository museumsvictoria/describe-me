using System.ComponentModel.DataAnnotations;

namespace DescribeMe.Web.ViewModels
{
    public class AccountManageInput
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }
}