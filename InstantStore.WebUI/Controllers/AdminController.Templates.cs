using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.Models;
using InstantStore.Domain.Exceptions;
using System.Net;
using InstantStore.WebUI.ViewModels.Factories;

namespace InstantStore.WebUI.Controllers
{
    public partial class AdminController
    {
        public ActionResult Templates()
        {
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Templates);
            this.ViewData["ControlPanelViewModel"] = new ControlPanelViewModel(this.repository, ControlPanelPage.Templates);
            var templates = this.repository.GetTemplates().Select(x => new TemplateViewModel() { Id = x.Id, Name = x.Name }).ToList();
            return this.Authorize() ?? this.View(templates);
        }

        public ActionResult NewTemplateProperty(string text, Guid? templateId)
        {
            if (templateId == null || templateId == Guid.Empty || string.IsNullOrWhiteSpace(text))
            {
                return this.HttpNotFound();
            }

            var property = new CustomProperty
            {
                TemplateId = templateId.Value,
                Name = text
            };

            property.Id = this.repository.AddNewCustomProperty(property);
            
            this.ViewData["PropertyIndex"] = 1;
            return this.View("EditorTemplates/CustomProperty", property);
        }

        public ActionResult DeleteProperty(Guid? id)
        {
            if (id == null || id == Guid.Empty)
            {
                return this.HttpNotFound();
            }

            this.repository.DeleteCustomProperty(id.Value);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult UpdateProperty(Guid? id, string data)
        {
            if (id == null || id == Guid.Empty || string.IsNullOrEmpty(data))
            {
                return this.HttpNotFound();
            }

            this.repository.UpdateCustomProperty(id.Value, data);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpGet]
        public ActionResult Template(string a, Guid? id)
        {
            PropertyTemplate template;
            if (id == null || id == Guid.Empty || (template = this.repository.GetTemplateById(id.Value)) == null)
            {
                return this.RedirectToAction("Templates");
            }

            if (string.Equals("delete", a))
            {
                this.repository.DeleteTemplate(id.Value);
                return this.RedirectToAction("Templates");
            }
            
            this.ViewData["TemplatesList"] = this.repository.GetTemplates().Select(x => new TemplateViewModel() { Id = x.Id, Name = x.Name }).ToList();
            return this.Authorize() ?? this.View(new TemplateViewModel(template, repository));
        }

        [HttpPost]
        public ActionResult Template(TemplateViewModel templateViewModel, string a, Guid? id)
        {
            if (templateViewModel == null || string.IsNullOrWhiteSpace(templateViewModel.Name))
            {
                return this.HttpNotFound();
            }

            if (templateViewModel.Id == Guid.Empty)
            {
                templateViewModel.Id = this.repository.AddNewTemplate(new PropertyTemplate()
                {
                    Name = templateViewModel.Name
                });
            }
            else
            {
                var propertyTemplate = this.repository.GetTemplateById(templateViewModel.Id);
                propertyTemplate.Name = templateViewModel.Name;
                this.repository.UpdateTemplate(propertyTemplate, templateViewModel.Properties, false);
            }

            this.ViewData["TemplatesList"] = this.repository.GetTemplates().Select(x => new TemplateViewModel() { Id = x.Id, Name = x.Name }).ToList();
            return this.Authorize() ?? this.View(templateViewModel);
        }
    }
}