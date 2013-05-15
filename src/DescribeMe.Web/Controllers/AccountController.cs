using System;
using System.Linq;
using System.Web.Mvc;
using DescribeMe.Core.Commands;
using DescribeMe.Core.Config;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Indexes;
using DescribeMe.Core.Infrastructure;
using DescribeMe.Web.Infrastructure;
using DescribeMe.Web.ViewModels;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using Raven.Client;
using Raven.Client.Linq;
using WebMatrix.WebData;

namespace DescribeMe.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IDocumentSession _documentSession;
        private readonly IMessageBus _messageBus;

        public AccountController(
            IDocumentSession documentSession,
            IMessageBus messageBus)
        {
            _documentSession = documentSession;
            _messageBus = messageBus;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            // try to log user in, redirect if user exists
            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: true) || User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }
            
            // User is new, ask for their desired membership name
            var loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);

            return View("ExternalLoginConfirmation", new AccountExternalLoginConfirmationViewModel
                        {
                            ReturnUrl = returnUrl,
                            ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName,
                            UserName = result.UserName, 
                            ExternalLoginData = loginData
                        });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(AccountExternalLoginConfirmationInput accountExternalLoginConfirmationInput)
        {
            string provider;
            string providerUserId;
            
            if (!OAuthWebSecurity.TryDeserializeProviderUserId(accountExternalLoginConfirmationInput.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(accountExternalLoginConfirmationInput.ReturnUrl);
            }

            if (ModelState.IsValid)
            {
                // User is new, create user and add to db
                var newUser = new User(
                    providerUserId,
                    provider,
                    accountExternalLoginConfirmationInput.UserName);

                newUser.AddRole(_documentSession.Load<Role>("roles/user"));

                _documentSession.Store(newUser);
                _documentSession.SaveChanges();

                // Log user in
                // HACK: Wait a tiny bit to ensure user is in database, because oauthwebsecurity NEEDS to check user exists first to add the forms authentication cookie.
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                stopwatch.Start();
                while (stopwatch.ElapsedMilliseconds < 1000) { }

                OAuthWebSecurity.Login(newUser.Provider, newUser.ProviderUserId, createPersistentCookie: true);
                    
                return RedirectToLocal(accountExternalLoginConfirmationInput.ReturnUrl);
            }

            return View(new AccountExternalLoginConfirmationViewModel
                        {
                            ReturnUrl = accountExternalLoginConfirmationInput.ReturnUrl,
                            ExternalLoginData = accountExternalLoginConfirmationInput.ExternalLoginData,
                            ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName,
                            UserName = accountExternalLoginConfirmationInput.UserName
                        });
        }
        
        [Authorize]
        public ActionResult Manage()
        {
            return View(new AccountManageViewModel
                {
                    UserName = User.Identity.Name
                });
        }

        [HttpPost]
        [Authorize]
        public ActionResult Manage(AccountManageInput accountManageInput)
        {
            var user = _documentSession
                    .Query<User, Users_ByName>()
                    .Where(x => x.Name == User.Identity.Name)
                    .FirstOrDefault();

            if (ModelState.IsValid && user != null)
            {
                var existingUser = _documentSession
                    .Query<User, Users_ByName>()
                    .Where(x => x.Name == accountManageInput.UserName)
                    .FirstOrDefault();

                if (existingUser == null)
                {
                    _messageBus.SendAsync(new UserUpdateCommand
                        {
                            Id = user.Id,
                            Name = accountManageInput.UserName
                        });

                    WebSecurity.Logout();

                    return RedirectToAction("ManageSuccess", "Account");
                }

                ModelState.AddModelError("UserName", user.Id == existingUser.Id ? "Please enter a different user name to your current one." : "User name already exists. Please enter a different user name.");
            }           

            return View(new AccountManageViewModel
                {
                    UserName = accountManageInput.UserName
                });
        }

        public ActionResult ManageSuccess()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            WebSecurity.Logout();

            return RedirectToAction("Describe", "Home");
        }

        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Describe", "Home");
        }
    }
}