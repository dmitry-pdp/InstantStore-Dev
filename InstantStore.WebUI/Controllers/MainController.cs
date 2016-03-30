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
        private static Dictionary<string, byte[]> thumbnailCache = new Dictionary<string, byte[]>();

        public ActionResult Index()
        {
            this.Initialize(Guid.Empty);
            this.ViewData["CategoryTilesViewModel"] = CategoryViewModelFactory.GetPriorityCategories(repository, this.Request);
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

            var key = id.ToString() + "_" + size;

            byte[] image;
            if (!thumbnailCache.TryGetValue(key, out image))
            {
                var thumbnail = this.repository.GetImageThumbnailById(id);
                if (thumbnail == null)
                {
                    return this.HttpNotFound();
                }
                
                image = (size == "l" ? thumbnail.LargeThumbnail : thumbnail.SmallThumbnail).ToArray();
                thumbnailCache.Add(key, image);
            }

            var stream = new MemoryStream(image);
            return new FileStreamResult(stream, "image/png");
        }

        protected override void Initialize(Guid pageId, PageIdentity pageIdentity = PageIdentity.Unknown, bool promoteProducts = true)
        {
            base.Initialize(pageId);
            this.ViewData["NavigationMenuViewModel"] = MenuViewModelFactory.CreateNavigationMenu(repository, null, this.Request);
            this.ViewData["BreadcrumbViewModel"] = MenuViewModelFactory.CreateBreadcrumb(repository, null);
        }
    }
}
