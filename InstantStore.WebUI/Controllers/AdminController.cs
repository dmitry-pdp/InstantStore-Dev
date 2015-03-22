using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.Models;

namespace InstantStore.WebUI.Controllers
{
    public class AdminController : Controller
    {
        private readonly IRepository repository;
        private readonly SettingsViewModel settingsViewModel;

        public AdminController(IRepository repository)
        {
            this.repository = repository;
            this.settingsViewModel = new SettingsViewModel(this.repository);
        }

        public ActionResult Users()
        {
            this.ViewData["ControlPanelViewModel"] = new ControlPanelViewModel(this.repository);
            this.ViewData["UsersListViewModel"] = new UsersListViewModel(this.repository);
            return this.Authorize() ?? View();
        }

        public ActionResult User(Guid id, bool? activate, bool? unblock, bool? block)
        {
            this.ViewData["UsersListViewModel"] = new UsersListViewModel(this.repository, id);
            if (activate != null && activate.Value)
            {
                this.repository.ActivateUser(id);
                return this.RedirectToAction("Users");
            }
            if (unblock != null && unblock.Value)
            {
                this.repository.UnblockUser(id);
                return this.RedirectToAction("Users");
            }
            if (block != null && block.Value)
            {
                this.repository.BlockUser(id);
                return this.RedirectToAction("Users");
            }
            return this.Authorize() ?? this.View(new UserViewModel(this.repository, id));
        }

        public ActionResult Dashboard()
        {
            return this.Authorize() ?? this.View();
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
    }
}
