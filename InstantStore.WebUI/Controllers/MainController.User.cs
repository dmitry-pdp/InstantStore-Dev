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
using InstantStore.WebUI.Resources;

namespace InstantStore.WebUI.Controllers
{
    public partial class MainController
    {
        [HttpGet]
        public ActionResult NewUser()
        {
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            return View(new UserViewModel());
        }

        [HttpPost]
        public ActionResult NewUser(UserViewModel user)
        {
            // TODO: DDoS vulnerability. Throttling needs to be added here.
            if (!this.ModelState.IsValid)
            {
                return this.View(user);
            }

            this.repository.AddUser(new User
            {
                Name = user.Name,
                Email = user.Email,
                Company = user.Company,
                Phonenumber = user.Phonenumber,
                City = user.City,
                Password = user.Password,
                IsAdmin = false,
                IsActivated = false,
                IsBlocked = false,
                IsPaymentCash = true,
                DefaultCurrencyId = null
            });

            return new RedirectResult("~/main/NewUserConfirmation");
        }

        public ActionResult NewUserConfirmation()
        {
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            return this.View();
        }

        public ActionResult Profile()
        {
            var user = this.InitializeCommonControls(Guid.Empty, PageIdentity.UserProfile);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            return this.View(UserViewModelFactory.CreateUserViewModel(user));
        }

        [HttpPost]
        public ActionResult Profile(UserViewModelBase userViewModel)
        {
            var user = this.InitializeCommonControls(Guid.Empty, PageIdentity.UserProfile);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            if (this.ModelState.IsValid)
            {
                user.City = userViewModel.City;
                user.Company = userViewModel.Company;
                user.Email = userViewModel.Email;
                user.Phonenumber = userViewModel.Phonenumber;
                this.repository.UpdateUser(user);

                return this.RedirectToAction("Index");
            }

            return this.View(userViewModel);
        }

        public ActionResult Login()
        {
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            return View();
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
    }
}