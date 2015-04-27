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
    public partial class AdminController
    {
        public ActionResult Index()
        {
            this.ViewData["ControlPanelViewModel"] = new ControlPanelViewModel(this.repository, ControlPanelPage.Dashboard);
            return this.Authorize() ?? this.View("Dashboard");
        }
    }
}