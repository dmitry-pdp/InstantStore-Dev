using System;
using System.Collections.Generic;
using System.Globalization;
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
    [CustomAuthorization(true)]
    public partial class AdminController : Controller
    {
        private static CultureInfo russianCulture = new System.Globalization.CultureInfo("ru-RU");

        private readonly IRepository repository;
        private readonly SettingsViewModel settingsViewModel;

        public AdminController()
        {
            this.repository = new LinqRepository();
            this.settingsViewModel = new SettingsViewModel(this.repository);
            this.ViewData["RenderCustomLeftColumn"] = true;
        }
    }
}
