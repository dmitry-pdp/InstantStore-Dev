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
        public ActionResult Pages()
        {
            this.ViewData["ControlPanelViewModel"] = new ControlPanelViewModel(this.repository, ControlPanelPage.Pages);
            return this.Authorize() ?? this.View(repository.GetPages(null).Select(page => new PageViewModel(page)).ToList());
        }

        public ActionResult NewPage()
        {
            this.ViewData["ControlPanelViewModel"] = new ControlPanelViewModel(this.repository, ControlPanelPage.Pages);
            return this.Authorize() ?? this.View(new PageViewModel(this.repository));
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult NewPage(PageViewModel pageViewModel)
        {
            if (this.ModelState.IsValid)
            {
                Guid? parentId = pageViewModel.ParentCategoryId == Guid.Empty ? (Guid?)null : pageViewModel.ParentCategoryId;
                repository.NewPage(new ContentPage {
                    Name = pageViewModel.Name,
                    Text = pageViewModel.Text,
                    ParentId = parentId,
                    ContentType = 1,
                    Position = repository.GetPages(parentId).Count + 1
                });
            }

            pageViewModel.InitializeRootCategory(this.repository);
            this.ViewData["ControlPanelViewModel"] = new ControlPanelViewModel(this.repository, ControlPanelPage.Pages);
            return this.Authorize() ?? this.View(pageViewModel);
        }

        public ActionResult Page(Guid id)
        {
            var page = repository.GetPageById(id);
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            return this.View(new PageViewModel(page));
        }
    }
}