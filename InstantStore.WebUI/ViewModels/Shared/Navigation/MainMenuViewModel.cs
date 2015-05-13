using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.Models;
using System.Web.Http.Routing;

namespace InstantStore.WebUI.ViewModels
{
    public class MainMenuViewModel
    {
        public IList<MenuItemViewModel> Menu { get; set; }
        public IList<MenuItemViewModel> MetaMenu { get; set; }
    }
}