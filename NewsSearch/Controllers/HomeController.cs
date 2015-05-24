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

        private Dictionary<string, string> SearchNews(string p)
        {
            return new Dictionary<string, string>
            {
                {"Source Name 1", "News head line 1"}, 
                {"Source Name 1", "News head line 2"}, 
                {"Source Name 1", "News head line 3"}, 
                {"Source Name 2", "News head line 1"}, 
                {"Source Name 2", "News head line 2"}, 
                {"Source Name 2", "News head line 3"}, 
                {"Source Name 2", "News head line 4"}, 
                {"Source Name 2", "News head line 5"}
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