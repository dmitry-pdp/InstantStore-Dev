using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.Domain.Helpers;
using InstantStore.WebUI.Models;
using InstantStore.WebUI.Resources;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.ViewModels.Factories;

namespace InstantStore.WebUI.Controllers
{
    // TODO: DDoS vulnerability. Throttling needs to be added here.
    public partial class MainController : ControllerBase
    {
        public ActionResult Index()
        {
            this.Initialize(Guid.Empty);
            this.ViewData["NavigationMenuViewModel"] = MenuViewModelFactory.CreateNavigationMenu(repository, null);
            this.ViewData["BreadcrumbViewModel"] = MenuViewModelFactory.CreateBreadcrumb(repository, null);
            this.ViewData["CategoryTilesViewModel"] = CategoryViewModelFactory.GetPriorityCategories(repository);
            return View();
        }

        public ActionResult Feedback()
        {
            this.Initialize(Guid.Empty, PageIdentity.Feedback, false);
            return View();
        }

        [HttpPost]
        public ActionResult SubmitFeedback(Feedback feedback)
        {
            if (!string.IsNullOrWhiteSpace(feedback.Name) &&
                !string.IsNullOrWhiteSpace(feedback.Email) &&
                !string.IsNullOrWhiteSpace(feedback.Message))
            {
                this.repository.AddFeedback(feedback);
            }

            return this.RedirectToAction("Index");
        }

        public ActionResult GetImage(Guid id)
        {
            var image = this.repository.GetImageById(id);
            if (image == null)
            {
                return this.HttpNotFound();
            }

            var stream = new MemoryStream(image.Image1.ToArray());
            return new FileStreamResult(stream, image.ImageContentType);
        }

        public ActionResult Thumbnail(Guid id, string size)
        {
            if (size != "l" && size != "s")
            {
                return this.HttpNotFound();
            }

            var thumbnail = this.repository.GetImageThumbnailById(id);
            if (thumbnail == null)
            {
                return this.HttpNotFound();
            }

            var stream = new MemoryStream((size == "l" ? thumbnail.LargeThumbnail : thumbnail.SmallThumbnail).ToArray());
            return new FileStreamResult(stream, "image/png");
        }
    }
}
