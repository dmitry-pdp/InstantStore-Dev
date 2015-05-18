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

        private User InitializeCommonControls(Guid pageId, PageIdentity page = PageIdentity.Unknown, bool showProducts = true)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, repository);

            bool isAuthenticated = user != null;
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateDefaultMenu(repository, pageId, user, page);
            this.ViewData["ShowLeftRailLogin"] = !isAuthenticated;

            if (showProducts)
            {
                this.ViewData["MediaListViewModel"] = CategoryViewModelFactory.CreatePopularProducts(repository, null);
            }

            return user;
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
            this.InitializeCommonControls(Guid.Empty, PageIdentity.Feedback, false);
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

                var productViewModel = new ProductViewModel(this.repository, id.Value, parentCategoryId, user);
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

        public ActionResult History(int offset = 0, int count = 25)
        {
            var user = this.InitializeCommonControls(Guid.Empty, PageIdentity.History);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            return this.View(new OrderHistoryListViewModel(this.repository, user, offset, count));
        }

        public ActionResult HistoryOrderDetails(Guid? id)
        {
            var user = this.InitializeCommonControls(Guid.Empty);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            Order order = id != null && id.Value != Guid.Empty ? this.repository.GetOrderById(id.Value) : null;
            if (order == null)
            {
                return this.HttpNotFound();
            }

            return this.View(new OrderDetailsViewModel().Load(this.repository, order, user));
        }

        public ActionResult Orders(Guid? id, string a)
        {
            this.InitializeCommonControls(Guid.Empty, PageIdentity.Cart);
            var user = UserIdentityManager.GetActiveUser(this.Request, this.repository);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            if (a == "delete" && id != null)
            {
                repository.DeleteOrderProduct(id.Value);
            }

            return this.View("Orders", new OrderDetailsViewModel(this.repository, user));
        }

        [HttpPost]
        public ActionResult Recalculate(OrderDetailsViewModel viewModel)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, this.repository);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            var order = this.repository.GetOrderById(viewModel.Id);
            if (order == null || order.Status != (int)OrderStatus.Active)
            {
                return this.HttpNotFound();
            }

            var orderProducts = this.repository.GetProductsForOrder(order.Id);
            if (orderProducts.Count != viewModel.Products.Count || !orderProducts.All(x => viewModel.Products.Any(y => y.Id == x.Key.Id)))
            {
                throw new ApplicationException("Cart contents is inconsistent.");
            }

            // update the cart items count.
            foreach (var orderProduct in viewModel.Products)
            {
                var orderProductPair = orderProducts.Where(x => x.Key.Id == orderProduct.Id).First();
                var product = orderProductPair.Value;
                var orderItem = orderProductPair.Key;

                if (!product.IsAvailable)
                {
                    continue;
                }

                orderItem.Count = orderProduct.Count;
                orderItem.Price = product.GetPriceForUser(user, this.repository.GetExchangeRates());
                orderItem.PriceCurrencyId = user.DefaultCurrencyId.Value;
                this.repository.UpdateOrderProduct(orderItem);
            }

            return this.Orders(null, null);
        }

        [HttpPost]
        public ActionResult PlaceOrder(OrderDetailsViewModel viewModel)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, this.repository);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            var order = this.repository.GetOrderById(viewModel.Id);
            if (order == null || order.Status != (int)OrderStatus.Active)
            {
                return this.HttpNotFound();
            }

            var orderProducts = this.repository.GetProductsForOrder(order.Id);
            if (orderProducts.Count != viewModel.Products.Count || !orderProducts.All(x => viewModel.Products.Any(y => y.Id == x.Key.Id)))
            {
                throw new ApplicationException("Cart contents is inconsistent.");
            }

            var orderProductsToRemove = new List<Guid>();
            foreach (var orderProduct in orderProducts.Where(x => !viewModel.Products.Any(y => y.Id == x.Key.Id)))
            {
                this.repository.RemoveOrderProduct(orderProduct.Key.Id);
            }

            this.repository.SubmitOrder(order.Id);
            return this.RedirectToAction("Index");
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

        public ActionResult CopyOrder(Guid id)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, this.repository);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            this.repository.AddProductsFromOrder(id, user);

            return this.RedirectToAction("Orders");
        }
    }
}
