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
using InstantStore.WebUI.Resources;

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

        private void InitializeCommonControls(Guid pageId, bool showProducts = true)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, repository);

            bool isAuthenticated = user != null;
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateDefaultMenu(repository, pageId, user);
            this.ViewData["ShowLeftRailLogin"] = !isAuthenticated;

            if (showProducts)
            {
                this.ViewData["MediaListViewModel"] = CategoryViewModelFactory.CreatePopularProducts(repository, null);
            }
        }

        public ActionResult Index()
        {
            this.InitializeCommonControls(Guid.Empty);
            this.ViewData["NavigationMenuViewModel"] = MenuViewModelFactory.CreateNavigationMenu(repository, null);
            this.ViewData["BreadcrumbViewModel"] = MenuViewModelFactory.CreateBreadcrumb(repository, null);
            this.ViewData["CategoryTilesViewModel"] = CategoryViewModelFactory.GetPriorityCategories(repository);
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
            this.InitializeCommonControls(Guid.Empty, false);
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
                return this.Json(new { result = "error", message = StringResource.login_ErrorUserCredentials });
            }

            UserIdentityManager.ResetUser(this.Request, this.Response);

            var user = this.repository.Login(name, password);
            if (user == null)
            {
                return this.Json(new { result = "error", message = StringResource.login_ErrorUserCredentials });
            }

            if (!user.IsActivated)
            {
                return this.Json(new { result = "error", message = StringResource.login_ErrorUserNotActivated });
            }

            UserIdentityManager.AddUserSession(this.Response, user);

            return this.Json(new { result = "success" });
;
        }

        public ActionResult Logoff()
        {
            UserIdentityManager.ResetUser(this.Request, this.Response);
            return this.RedirectToAction("Index");
        }

        public ActionResult Page(Guid? id, Guid? parentCategoryId, int c = 100)
        {
            if (id == null || id == Guid.Empty)
            {
                return this.RedirectToAction("Index");
            }

            this.InitializeCommonControls(id.Value);

            Product product;

            var user = UserIdentityManager.GetActiveUser(this.Request, repository);

            var contentPage = this.repository.GetPageById(id.Value);
            if (contentPage != null)
            {
                // Page/Category view
                this.ViewData["BreadcrumbViewModel"] = MenuViewModelFactory.CreateBreadcrumb(repository, id);
                this.ViewData["NavigationMenuViewModel"] = MenuViewModelFactory.CreateNavigationMenu(repository, id);

                var viewModel = new PageViewModel(contentPage);
                if (viewModel.Attachment != null)
                {
                    viewModel.Attachment.CanEdit = false;
                }

                if (contentPage.IsCategory())
                {
                    this.ViewData["ProductTilesViewModel"] = CategoryViewModelFactory.GetProductsForCategory(repository, user, id.Value, c);
                }

                return this.View(viewModel);
            }
            else if((product = repository.GetProductById(id.Value)) != null)
            {
                if (parentCategoryId != null)
                {
                    this.ViewData["BreadcrumbViewModel"] = MenuViewModelFactory.CreateBreadcrumb(repository, parentCategoryId);
                }

                this.ViewData["MediaListViewModel"] = CategoryViewModelFactory.CreateSimilarProducts(repository, parentCategoryId);

                var productViewModel = new ProductViewModel(this.repository, id.Value, parentCategoryId);
                return this.View("Product", productViewModel);
            }
            else
            {
                return this.HttpNotFound();
            }
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

        public ActionResult History()
        {
            this.InitializeCommonControls(Guid.Empty);
            return this.View();
        }

        public ActionResult Orders()
        {
            this.InitializeCommonControls(Guid.Empty);
            return this.View();
        }

        [HttpPost]
        public ActionResult AddToCart(Guid id, int count = 1)
        {
            if (id == Guid.Empty)
            {
                return this.HttpNotFound();
            }

            var user = UserIdentityManager.GetActiveUser(this.Request, this.repository);
            if (user == null || !user.IsActivated)
            {
                return this.HttpNotFound();
            }

            this.repository.AddItemToCurrentOrder(user, id, count);

            return this.Json(new { result = "success" });
        }
    }
}
