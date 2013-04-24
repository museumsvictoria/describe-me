using System.Linq;
using System.Web.Mvc;
using DescribeMe.Core.Commands;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Indexes;
using DescribeMe.Core.Infrastructure;
using DescribeMe.Web.ViewModels;
using Raven.Client;
using Raven.Client.Linq;

namespace DescribeMe.Web.Controllers
{
    public class HomeController : Controller
    {        
        private readonly IDocumentSession _documentSession;
        private readonly IMessageBus _messageBus;

        public HomeController(
            IDocumentSession documentSession,
            IMessageBus messageBus)
        {
            _documentSession = documentSession;
            _messageBus = messageBus;
        }

        public ActionResult Index()
        {
            ViewBag.HideLogin = true;

            return View();
        }

        public ActionResult Describe(string id, string tag)
        {
            Image image;

            if (!string.IsNullOrWhiteSpace(id))
            {
                image = _documentSession.Load<Image>(id);
                
                if (image == null)
                {
                    return View("ImageNotFound");
                }
            }
            else
            {
                var imageQuery = _documentSession.Query<Image, Images_NotDescribed>();

                if (!string.IsNullOrWhiteSpace(tag))
                    imageQuery = imageQuery.Where(x => x.Tags.Any(imageTag => imageTag == tag));

                if (!string.IsNullOrWhiteSpace((string)TempData["skipId"]))
                    imageQuery = imageQuery.Where(x => x.Id != (string)TempData["skipId"]);

                image = imageQuery
                    .Customize(x => x.RandomOrdering())
                    .FirstOrDefault();

                // If there are no more images to describe in this tag
                if (!string.IsNullOrWhiteSpace(tag) && image == null)
                {
                    return RedirectToAction("Describe");
                }
            }

            return View(new ImageViewModel
                        {
                            Image = image,
                            Tag = tag
                        });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Describe(HomeDescribeInput homeDescribeInput)
        {
            if (ModelState.IsValid == false)
                return Describe(homeDescribeInput.Id, homeDescribeInput.Tag);

            _messageBus.SendAsync(new ImageDescribeCommand
                {
                    Id = homeDescribeInput.Id,
                    UserAltDescription = homeDescribeInput.UserAltDescription,
                    User = _documentSession.Query<User, Users_ByName>().Where(x => x.Name == User.Identity.Name).FirstOrDefault()
                });

            TempData["skipId"] = homeDescribeInput.Id;
            TempData["message"] = "Thank you for your description.";
            
            return RedirectToAction("Describe", new { tag = homeDescribeInput.Tag });
        }

        [Authorize(Roles = "administrator, moderator")]
        public ActionResult Review(string id, string tag)
        {
            Image image;

            if (!string.IsNullOrWhiteSpace(id))
            {
                image = _documentSession.Load<Image>(id);

                if (image == null)
                {
                    return View("ImageNotFound");
                }
            }
            else
            {
                var imageQuery = _documentSession.Query<Image, Images_NotApproved>();

                if (!string.IsNullOrWhiteSpace(tag))
                    imageQuery = imageQuery.Where(x => x.Tags.Any(imageTag => imageTag == tag));

                if (!string.IsNullOrWhiteSpace((string)TempData["skipId"]))
                    imageQuery = imageQuery.Where(x => x.Id != (string)TempData["skipId"]);

                image = imageQuery
                    .Customize(x => x.RandomOrdering())
                    .FirstOrDefault();

                // If there are no more images to describe in this tag
                if (!string.IsNullOrWhiteSpace(tag) && image == null)
                {
                    return RedirectToAction("Review");
                }
            }

            return View(new ImageViewModel
            {
                Image = image,
                Tag = tag
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "administrator, moderator")]
        public ActionResult Approve(HomeDescribeInput homeApproveInput)
        {
            if (ModelState.IsValid == false)
                return Review(homeApproveInput.Id, homeApproveInput.Tag);

            _messageBus.SendAsync(new ImageApproveCommand
            {
                Id = homeApproveInput.Id,
                UserAltDescription = homeApproveInput.UserAltDescription
            });

            TempData["skipId"] = homeApproveInput.Id;
            TempData["message"] = "Description approved.";

            return RedirectToAction("Review", new { tag = homeApproveInput.Tag });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "administrator, moderator")]
        public ActionResult Reject(HomeRejectInput homeRejectInput)
        {
            _messageBus.SendAsync(new ImageRejectCommand
            {
                Id = homeRejectInput.Id
            });

            TempData["skipId"] = homeRejectInput.Id;
            TempData["message"] = "Description rejected.";

            return RedirectToAction("Review", new { tag = homeRejectInput.Tag });
        }

        public ActionResult Examples()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult Statistics()
        {
            var statistics = _documentSession
                .Query<Images_Statistics.ReduceResult, Images_Statistics>()
                .FirstOrDefault() ?? new Images_Statistics.ReduceResult();

            return PartialView(new StatisticsViewModel
            {                
                DescribedImageCount = statistics.DescibedImageCount,
                UnDescribedImageCount = statistics.UnDescibedImageCount,
                ApprovedImageCount = statistics.ApprovedImageCount,
                UnApprovedImageCount = statistics.UnApprovedImageCount
            });
        }

        [ChildActionOnly]
        public ActionResult UserStatistics()
        {
            return PartialView(_documentSession
                .Query<Images_DescribedByCount.ReduceResult, Images_DescribedByCount>()
                .Where(x => x.UserName == User.Identity.Name)
                .ToList());
        }

        [ChildActionOnly]
        public ActionResult LeaderBoard()
        {
            return PartialView(_documentSession
                .Query<Images_DescribedByCount.ReduceResult, Images_DescribedByCount>()
                .Take(10)
                .OrderByDescending(x => x.Count)
                .ToList());
        }

        public ActionResult FilterTags(string term)
        {
            var wildcardTerm = string.Format("{0}*", term);

            return Json(_documentSession
                .Query<Tags_Count.ReduceResult, Tags_Count>()
                .Search(x => x.Tag, wildcardTerm, 1, SearchOptions.Or, EscapeQueryOptions.AllowPostfixWildcard)
                .Take(15)
                .ToList());
        }
    }
}