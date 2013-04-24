using System.Net;
using System.Web.Mvc;

namespace DescribeMe.Web.Controllers
{
    public class ErrorController : Controller
    {        
        public ActionResult PageNotFound()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return View();
        }
    }
}