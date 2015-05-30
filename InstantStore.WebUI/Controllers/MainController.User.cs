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
        public ActionResult NewUser()
        {
            this.Initialize(Guid.Empty, PageIdentity.Unknown);
            return View(new UserViewModel { AvailableCurrencies = this.GetAvailableCurrencyList() });
        }

        [HttpPost]
        public ActionResult NewUser(UserViewModel userViewModel)
        {
            // TODO: DDoS vulnerability. Throttling needs to be added here.
            using (var context = new InstantStoreDataContext())
            {
                if (context.Users.ToList().Any(
                    user =>
                        string.Equals(user.Name, userViewModel.Name, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(user.Email, userViewModel.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    this.ModelState.AddModelError(string.Empty, StringResource.UserAlreadyExists);
                }
            }

            if (!this.ModelState.IsValid)
            {
                this.Initialize(Guid.Empty, PageIdentity.Unknown);
                userViewModel.AvailableCurrencies = this.GetAvailableCurrencyList();
                return this.View(userViewModel);
            }

            var newUser = new User
            {
                Name = userViewModel.Name,
                Email = userViewModel.Email,
                Company = userViewModel.Company,
                Phonenumber = userViewModel.Phonenumber,
                City = userViewModel.City,
                Password = userViewModel.Password,
                IsAdmin = false,
                IsActivated = false,
                IsBlocked = false,
                IsPaymentCash = true,
                DefaultCurrencyId = userViewModel.Currency
            };

            this.repository.AddUser(newUser);

            EmailManager.Send(newUser, this.repository, EmailType.EmailNewUserRegistration);
            EmailManager.Send(newUser, this.repository, EmailType.EmailNewUserNotification);

            return this.RedirectToAction("NewUserConfirmation");
        }

        public ActionResult NewUserConfirmation()
        {
            this.Initialize(Guid.Empty, PageIdentity.Unknown);
            return this.View();
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
        }

        public ActionResult Logoff()
        {
            UserIdentityManager.ResetUser(this.Request, this.Response);
            return this.RedirectToAction("Index");
        }
    }
}