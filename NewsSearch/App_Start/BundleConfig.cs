using System.Web;
using System.Web.Optimization;

namespace NewsSearch
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleStyles(bundles);
            BundleScripts(bundles);
        }

        private static void BundleStyles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/content/css").Include(
                //"~/Content/cssfonts.css",
                //"~/Content/cssbase.css",
                //"~/Content/cssreset.css",
                "~/Content/sitebase.css"));

            bundles.Add(new StyleBundle("~/content/css/bootstrap").Include(
                "~/Content/bootstrap.min.css"));
        }

        private static void BundleScripts(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/sitescripts").Include(
                "~/Scripts/Site/sn-main.js"));

            bundles.Add(new ScriptBundle("~/bundles/searchnews").Include(
                "~/Scripts/Site/sn-searchnews.js"));
        }
    }
}