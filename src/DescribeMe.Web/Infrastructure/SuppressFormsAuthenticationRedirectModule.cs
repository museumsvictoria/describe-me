using System;
using System.Web;
using System.Web.Mvc;

namespace DescribeMe.Web.Infrastructure
{
    /// <summary>
    /// Supress forms authentication 401 hijacking, from Haacked https://github.com/Haacked/CodeHaacks/tree/master/src/AspNetHaack
    /// </summary>
    public class SuppressFormsAuthenticationRedirectModule : IHttpModule
    {
        private static readonly object SuppressAuthenticationKey = new Object();

        public static void SuppressAuthenticationRedirect(HttpContext context)
        {
            context.Items[SuppressAuthenticationKey] = true;
        }

        public static void SuppressAuthenticationRedirect(HttpContextBase context)
        {
            context.Items[SuppressAuthenticationKey] = true;
        }

        public void Init(HttpApplication context)
        {
            context.PostReleaseRequestState += OnPostReleaseRequestState;
            context.EndRequest += OnEndRequest;
        }

        private void OnPostReleaseRequestState(object source, EventArgs args)
        {
            var context = (HttpApplication)source;
            var response = context.Response;
            var request = context.Request;

            if (response.StatusCode == 401)
            {
                SuppressAuthenticationRedirect(context.Context);
            }
        }

        private void OnEndRequest(object source, EventArgs args)
        {
            var context = (HttpApplication)source;
            var response = context.Response;

            if (context.Context.Items.Contains(SuppressAuthenticationKey))
            {
                response.StatusCode = 401;
                response.RedirectLocation = "";
            }
        }

        public void Dispose()
        {
        }
    }
}