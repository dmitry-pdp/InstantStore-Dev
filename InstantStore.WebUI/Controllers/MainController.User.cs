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
    }
}