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
        private const string newUserSessionId = "User.NewUserId";

        public ActionResult NewUser()
        {
            this.Initialize(Guid.Empty, PageIdentity.Unknown);
            return View("NewUserStep1", new UserAuthViewModel());
        }

        [HttpPost]
        public ActionResult NewUser(UserAuthViewModel userViewModel)
        {
            // TODO: DDoS vulnerability. Throttling by ip needs to be added here.

            if (userViewModel == null)
            {
                return this.NewUser();
            }

            bool userExists = false;

            using (var context = new InstantStoreDataContext())
            {
                if (context.Users.ToList().Any(
                    user =>
                        string.Equals(user.Name, userViewModel.Name, StringComparison.OrdinalIgnoreCase) /* ||
                        string.Equals(user.Email, userViewModelBasic.Email, StringComparison.OrdinalIgnoreCase) */))
                {
                    userExists = true;
                    this.ModelState.AddModelError(string.Empty, StringResource.UserAlreadyExists);
                }
            }

            this.Initialize(Guid.Empty, PageIdentity.Unknown);

            if (!this.ModelState.IsValid)
            {
                userViewModel.Password = "";
                userViewModel.ConfirmPassword = "";
                return this.View("NewUserStep1", userViewModel);
            }

            userViewModel.UserId = Guid.NewGuid();

            this.Session[newUserSessionId] = userViewModel;
            return this.View("NewUserStep2", new UserViewModel { NewUserId = userViewModel.UserId, Name = userViewModel.Name });
        }

        [HttpPost]
        public ActionResult NewUser2(UserViewModel userViewModel)
        {
            if (userViewModel == null)
            {
                return this.NewUser();
            }

            var userAuthViewModel = this.Session[newUserSessionId] as UserAuthViewModel;
            if (userAuthViewModel == null || userAuthViewModel.UserId != userViewModel.NewUserId)
            {
                return this.NewUser();
            }
            
            var availableCurrencies = this.GetAvailableCurrencyList();

            var newUser = new User
            {
                Name = userAuthViewModel.Name,
                Email = userViewModel.Email,
                Company = userViewModel.Company,
                Phonenumber = userViewModel.Phonenumber,
                City = userViewModel.City,
                Password = userAuthViewModel.Password,
                IsAdmin = false,
                IsActivated = false,
                IsBlocked = false,
                IsPaymentCash = true,
                DefaultCurrencyId = availableCurrencies != null && availableCurrencies.Any() ? new Guid(availableCurrencies.First().Value) : (Guid?)null
            };

            this.repository.AddUser(newUser);

            EmailManager.Send(newUser, this.repository, EmailType.EmailNewUserRegistration);
            EmailManager.Send(newUser, this.repository, EmailType.EmailNewUserNotification);

            return this.RedirectToAction("NewUserConfirmation");
        }

        public ActionResult Login()
        {
            this.Initialize(Guid.Empty, PageIdentity.Unknown);
            return this.View();
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