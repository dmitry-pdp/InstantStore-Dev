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

        public ActionResult SomeWiredStuff()
        {
            return this.View();
        }

        public ActionResult Settings()
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, this.repository);
            if (user == null || !user.IsAdmin)
            {
                return this.HttpNotFound();
            }

            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SettingsUpdate(string headerHtml, string mainDocumentHtml, string footerHtml)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, this.repository);
            if (user == null || !user.IsAdmin)
            {
                return this.HttpNotFound();
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

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            {
                return this.HttpNotFound();
            }

            UserIdentityManager.ResetUser(this.Request, this.Response);

            var user = this.repository.Login(name, password);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            UserIdentityManager.AddUserSession(this.Response, user);

            return new RedirectResult("/");
        }

    }
}
