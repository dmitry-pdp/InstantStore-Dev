using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.Models;
using InstantStore.WebUI.ViewModels.Factories;

namespace InstantStore.WebUI.Controllers
{
    public partial class AdminController : Controller
    {
        private readonly IRepository repository;
        private readonly SettingsViewModel settingsViewModel;

        public AdminController(IRepository repository)
        {
            this.repository = repository;
            this.settingsViewModel = new SettingsViewModel(this.repository);
            this.ViewData["RenderCustomLeftColumn"] = true;
        }

        private ActionResult Authorize()
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, this.repository);
            if (user == null || !user.IsAdmin)
            {
                //return this.HttpNotFound();
            }

            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            return null;
        }

        public ActionResult Orders()
        {
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Orders);
            this.ViewData["ControlPanelViewModel"] = new ControlPanelViewModel(this.repository, ControlPanelPage.Orders);
            return this.Authorize() ?? this.View();
        }

        public ActionResult Offers()
        {
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Offers);
            this.ViewData["ControlPanelViewModel"] = new ControlPanelViewModel(this.repository, ControlPanelPage.Offers);
            return this.Authorize() ?? this.View();
        }
    }
}
