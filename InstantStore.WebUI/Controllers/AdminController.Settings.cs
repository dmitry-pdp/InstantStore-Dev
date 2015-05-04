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
    public partial class AdminController
    {
        public ActionResult Settings(string t = null)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, this.repository);
            if (user == null || !user.IsAdmin)
            {
                ////return this.HttpNotFound();
            }

            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Settings);
            this.ViewData["ControlPanelViewModel"] = new ControlPanelViewModel(this.repository, ControlPanelPage.Settings);
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            this.ViewData["ShowMainPage"] = "main" == t;
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SettingsUpdate(string headerHtml, string mainDocumentHtml, string footerHtml)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, this.repository);
            if (user == null || !user.IsAdmin)
            {
                ////return this.HttpNotFound();
            }

            if (settingsViewModel != null)
            {
                this.settingsViewModel.HeaderHtml = headerHtml;
                this.settingsViewModel.FooterHtml = footerHtml;
                this.settingsViewModel.MainDocumentHtml = mainDocumentHtml;

                this.settingsViewModel.ValidateAndSave();
            }

            return new RedirectResult("/");
        }

    }
}