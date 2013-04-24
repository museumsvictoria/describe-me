using System.Web.Optimization;

namespace DescribeMe.Web.Config
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            var scriptBundle = new ScriptBundle("~/js/jslib")
                .IncludeDirectory("~/js", "*.js", false)
                .IncludeDirectory("~/js/lib", "jquery*.js", false);
            bundles.Add(scriptBundle);

            var modernizrBundle = new ScriptBundle("~/js/modernizr")
                .Include("~/js/lib/modernizr-2.5.3.js");
            bundles.Add(modernizrBundle);

            var lessBundle = new Bundle("~/css/css").Include(
                "~/css/normalize.less",
                "~/css/style.less",
                "~/css/helpers.less",
                "~/css/print.less"
                );

            lessBundle.Transforms.Add(new LessTransform());
            bundles.Add(lessBundle);
        }
    }
}