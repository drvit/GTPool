using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NewsSearch.Models;

namespace NewsSearch.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var model = new SearchNewsViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(SearchNewsViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Results = SearchNews(model.SearchQuery);
                return View(model);
            }
            return RedirectToAction("Index");
        }

        private IList<NewsResults> SearchNews(string query)
        {
            return new List<NewsResults>
            {
                new NewsResults {Source = "Source Name 1", Headline = "News headline 1", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 1", Headline = "News headline 2", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 1", Headline = "News headline 3", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 2", Headline = "News headline 1", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 2", Headline = "News headline 2", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 2", Headline = "News headline 3", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 2", Headline = "News headline 4", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 2", Headline = "News headline 5", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 3", Headline = "News headline 1", Content = "News content related to the source and headline..."},
                new NewsResults {Source = "Source Name 3", Headline = "News headline 2", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 3", Headline = "News headline 3", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 3", Headline = "News headline 4", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 4", Headline = "News headline 1", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 4", Headline = "News headline 2", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 4", Headline = "News headline 3", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 4", Headline = "News headline 4", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 4", Headline = "News headline 5", Content = "News content related to the source and headline..."}, 
                new NewsResults {Source = "Source Name 5", Headline = "News headline 1", Content = "News content related to the source and headline..."}
            };
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}