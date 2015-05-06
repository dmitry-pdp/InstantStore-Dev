using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.Models;
using System.IO;
using InstantStore.WebUI.ViewModels.Factories;

namespace InstantStore.WebUI.Controllers
{
    public partial class MainController : Controller
    {
        private readonly IRepository repository;
        private readonly SettingsViewModel settingsViewModel;

        // Logged in users.
        // TODO: Replace with asp.net authentication.

        public MainController(IRepository repository)
        {
            this.repository = repository;
            this.settingsViewModel = new SettingsViewModel(this.repository);
        }

        public ActionResult Index()
        {
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateDefaultMenu(repository, Guid.Empty);
            this.ViewData["NavigationMenuViewModel"] = MenuViewModelFactory.CreateNavigationMenu(repository, null);
            this.ViewData["BreadcrumbViewModel"] = MenuViewModelFactory.CreateBreadcrumb(repository, null);
            this.ViewData["MediaListViewModel"] = CategoryViewModelFactory.CreatePopularProducts(repository, null);
            this.ViewData["CategoryTilesViewModel"] = repository.GetTopCategories();
            return View();
        }

        public ActionResult Login()
        {
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            return View();
        }

        public ActionResult SomeWiredStuff()
        {
            return this.View();
        }

        public ActionResult Feedback()
        {
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            return View();
        }

        [HttpPost]
        public ActionResult SubmitFeedback(Feedback feedback)
        {
            // TODO: DDoS vulnerability. Throttling needs to be added here.

            if (!string.IsNullOrWhiteSpace(feedback.Name) &&
                !string.IsNullOrWhiteSpace(feedback.Email) &&
                !string.IsNullOrWhiteSpace(feedback.Message))
            {
                this.repository.AddFeedback(feedback);
            }

            return new RedirectResult("/");
        }

        [HttpPost]
        public ActionResult LoginUser(string name, string password)
        {
            // TODO: DDoS vulnerability. Throttling needs to be added here.

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            {
                return this.HttpNotFound();
            }

            UserIdentityManager.ResetUser(this.Request, this.Response);

            var user = this.repository.Login(name, password);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            UserIdentityManager.AddUserSession(this.Response, user);

            return new RedirectResult("/");
        }

        public ActionResult Page(Guid? id, int c = 100)
        {
            if (id == null || id == Guid.Empty)
            {
                return this.RedirectToAction("Index");
            }

            var contentPage = this.repository.GetPageById(id.Value);

            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateDefaultMenu(repository, id.Value);
            this.ViewData["NavigationMenuViewModel"] = MenuViewModelFactory.CreateNavigationMenu(repository, id);
            this.ViewData["BreadcrumbViewModel"] = MenuViewModelFactory.CreateBreadcrumb(repository, id);

            var viewModel = new PageViewModel(contentPage);
            if (viewModel.Attachment != null)
            {
                viewModel.Attachment.CanEdit = false;
            }

            if (contentPage.IsCategory())
            {
                this.ViewData["ProductTilesViewModel"] = CategoryViewModelFactory.GetProductsForCategory(repository, id.Value, c);
            }

            return this.View(viewModel);
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

        public ActionResult Download(Guid id)
        {
            var attachment = this.repository.GetAttachmentById(id);
            if (attachment == null)
            {
                return this.HttpNotFound();
            }

            var stream = new MemoryStream(attachment.Content.ToArray());
            return new FileStreamResult(stream, attachment.ContentType);
        }
    }
}
