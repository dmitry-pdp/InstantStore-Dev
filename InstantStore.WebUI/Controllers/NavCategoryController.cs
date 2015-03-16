using InstantStore.Domain.Abstract;
using InstantStore.Domain.Entities;
using InstantStore.WebUI.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace InstantStore.WebUI.Controllers
{
    //Navigation by Categories
    public class NavCategoryController : Controller
    {
        IRepository repository;

        public NavCategoryController(IRepository repo)
        {
            repository = repo;
        }

        public PartialViewResult Menu(string category = null)
        {
            ViewBag.SelectedCategory = category;

            ProductCategory[] categories = repository.Products != null ?
                repository.Products
                        .Select(x => x.Category)
                        .DistinctBy(x => x.Name)
                        .OrderBy(x => x.Name).ToArray() : null;

            return PartialView(categories);
        }
    }
}
