using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Resources;

namespace InstantStore.WebUI.ViewModels
{
    public class ContentSummaryViewModel
    {
        public ContentSummaryViewModel(IRepository repository, Guid pageId)
        {
            this.PropertyData = new List<PropertyPair>();
            this.ActionsData = new List<ActionData>();
            this.Initialize(repository, pageId);
        }

        public List<PropertyPair> PropertyData { get; private set; }

        public List<ActionData> ActionsData { get; private set; }
 
        private void Initialize(IRepository repository, Guid pageId)
        {
            ContentPage page = null;
            if (pageId == Guid.Empty)
            {
                page = new ContentPage
                {
                    Id = Guid.Empty,
                    Name = StringResource.admin_PageTreeRoot,
                    ContentType = 0
                };
            }
            else
            {
                page = repository.GetPageById(pageId);
            }

            this.PropertyData.Add(new PropertyPair(StringResource.admin_PagesPropertiesName, page.Name));
            this.PropertyData.Add(new PropertyPair(StringResource.admin_PagesPropertiesType, StringResource.ResourceManager.GetString("admin_PagesPageType_" + ((ContentType)page.ContentType).ToString())));

            if ((ContentType)page.ContentType != ContentType.RootPage)
            {
                this.ActionsData.Add(new ActionData(StringResource.admin_PagesPropertiesActionMoveUp, "EditPage", "Admin", new { id = page.Id, action = "moveup" }, "glyphicon glyphicon-arrow-up"));
                this.ActionsData.Add(new ActionData(StringResource.admin_PagesPropertiesActionMoveDown, "EditPage", "Admin", new { id = page.Id, action = "movedown" }, "glyphicon glyphicon-arrow-down"));
            }

            this.ActionsData.Add(new ActionData(StringResource.admin_PagesPropertiesPreview, "Page", "Main", new { id = page.Id }, "glyphicon glyphicon-eye-open"));
            this.ActionsData.Add(new ActionData(StringResource.admin_PagesPropertiesEdit, "EditPage", "Admin", new { id = page.Id, action = "update" }, "glyphicon glyphicon-edit"));

            if ((ContentType)page.ContentType == ContentType.Category)
            {
                this.ActionsData.Add(new ActionData(StringResource.admin_PagesPropertiesActionEditProducts, "EditProducts", "Admin", new { id = page.Id }, "glyphicon glyphicon-list-alt"));
            }
            
            this.ActionsData.Add(new ActionData(StringResource.admin_PagesPropertiesDelete, "DeletePage", "Admin", new { id = page.Id }, "glyphicon glyphicon-trash"));
        }
    }

    public struct PropertyPair 
    {
        public PropertyPair(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name;

        public string Value;
    }

    public struct ActionData
    {
        public ActionData(string label, string action, string controller, object routeValues)
        {
            this.Label = label;
            this.Action = action;
            this.Controller = controller;
            this.RouteValues = routeValues;
            this.Glyph = null;
        }

        public ActionData(string label, string action, string controller, object routeValues, string glyph)
        {
            this.Label = label;
            this.Action = action;
            this.Controller = controller;
            this.RouteValues = routeValues;
            this.Glyph = glyph;
        }

        public string Label;

        public string Action;

        public string Controller;

        public object RouteValues;

        public string Glyph;
    }
}
