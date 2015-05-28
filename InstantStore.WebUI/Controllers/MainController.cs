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

            return this.RedirectToAction("Index");
        }
    }
}
