using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.ViewModels;

namespace InstantStore.WebUI.Controllers
{
    public class MainController : Controller
    {
        private readonly IRepository repository;
        private readonly SettingsViewModel settingsViewModel;

        public MainController(IRepository repository)
        {
            this.repository = repository;
            this.settingsViewModel = new SettingsViewModel(this.repository);
        }

        public ActionResult Index()
        {
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            return View();
        }

        public ActionResult Login()
        {
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            return View();
        }

        [HttpGet]
        public ActionResult NewUser()
        {
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            return View();
        }

        [HttpPost]
        public ActionResult NewUser(string name, string email, string company, string phonenumber, string city, string password, string passwordc)
        {
            // TODO: DDoS vulnerability. Throttling needs to be added here.

            if (!string.IsNullOrWhiteSpace(name) &&
                !string.IsNullOrWhiteSpace(password) &&
                string.Equals(password, passwordc, StringComparison.Ordinal))
            {
                this.repository.AddUser(new User
                {
                    Name = name, 
                    Email = email,
                    Company = company,
                    Phonenumber = phonenumber,
                    City = city,
                    Password = PasswordHash.PasswordHash.CreateHash(password),
                    IsAdmin = false,
                    IsActivated = false,
                    IsBlocked = false,
                    IsPaymentCash = true,
                    DefaultCurrencyId = null
                });
            
                // TODO: Add session for the user.
            }

            return new RedirectResult("/");
        }

        public ActionResult Settings()
        {
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SettingsUpdate(string headerHtml, string mainDocumentHtml, string footerHtml)
        {
            if (settingsViewModel != null)
            {
                this.settingsViewModel.HeaderHtml = headerHtml;
                this.settingsViewModel.FooterHtml = footerHtml;
                this.settingsViewModel.MainDocumentHtml = mainDocumentHtml;

                this.settingsViewModel.ValidateAndSave();
            }
            
            return new RedirectResult("/");
        }

        public ActionResult Feedback()
        {
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
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

            return new RedirectResult("/");
        }
    }
}
