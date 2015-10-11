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
    public abstract class ControllerBase : Controller
    {
        protected readonly IRepository repository;
        protected readonly SettingsViewModel settingsViewModel;

        protected User currentUser;

        protected ControllerBase()
        {
            this.repository = new LinqRepository();
            this.settingsViewModel = new SettingsViewModel(this.repository);
        }

        protected virtual void Initialize(Guid pageId, PageIdentity pageIdentity = PageIdentity.Unknown, bool promoteProducts = true)
        {
            this.currentUser = this.HttpContext.CurrentUser() ?? UserIdentityManager.GetActiveUser(this.Request, this.repository);
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateDefaultMenu(this.repository, pageId, currentUser, pageIdentity);
            this.ViewData["ShowLeftRailLogin"] = currentUser == null;
            //this.ViewData["MediaListViewModel"] = promoteProducts ? CategoryViewModelFactory.CreatePopularProducts(this.repository, null) : null;
            this.ViewData["HttpReferrer"] = this.Request.UrlReferrer != null ? this.Request.UrlReferrer.ToString() : null;
        }

        protected IList<SelectListItem> GetAvailableCurrencyList()
        {
            var currencies = this.repository.GetCurrencies() ?? new List<Currency>();
            return currencies.Select(x =>
            {
                return new SelectListItem()
                {
                    Text = x.Text,
                    Value = x.Id.ToString()
                };
            })
            .ToList();
        }
    }
}
