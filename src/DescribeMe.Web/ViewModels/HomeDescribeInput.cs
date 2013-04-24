using System.ComponentModel.DataAnnotations;

namespace DescribeMe.Web.ViewModels
{
    public class HomeDescribeInput
    {        
        public string Id { get; set; }

        public string Tag { get; set; }

        [Required(ErrorMessage = "You must enter a description")]
        public string UserAltDescription { get; set; }
    }
}
