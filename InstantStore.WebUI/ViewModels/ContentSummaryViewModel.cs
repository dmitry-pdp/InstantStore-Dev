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

            var contentType = (ContentType)page.ContentType;

            this.PropertyData.Add(new PropertyPair(StringResource.admin_PagesPropertiesName, page.Name));
            this.PropertyData.Add(new PropertyPair(StringResource.admin_PagesPropertiesType, StringResource.ResourceManager.GetString("admin_PagesPageType_" + ((ContentType)page.ContentType).ToString())));
            this.PropertyData.Add(new PropertyPair(StringResource.admin_PagePropertiesPagePosition, page.Position.ToString()));

            if (contentType == ContentType.Page)
            {
                Attachment attachment = page.AttachmentId != null ? repository.GetAttachmentById(page.AttachmentId.Value) : null;
                this.PropertyData.Add(new PropertyPair(StringResource.admin_PagePropertiesPageAttachment, attachment != null ? attachment.Name : StringResource.admin_PagePropertiesPageNone));
            }

            if (contentType == ContentType.RootPage)
            {
                this.ActionsData.Add(new ActionData(StringResource.admin_PagesPropertiesPreview, "Page", "Main", new { id = page.Id }, "glyphicon glyphicon-eye-open"));
            }
            else
            {
                this.ActionsData.Add(new ActionData(StringResource.admin_PagesPropertiesActionMoveUp, "Page", "Admin", new { id = page.Id, a = "moveup" }, "glyphicon glyphicon-arrow-up"));
                this.ActionsData.Add(new ActionData(StringResource.admin_PagesPropertiesActionMoveDown, "Page", "Admin", new { id = page.Id, a = "movedown" }, "glyphicon glyphicon-arrow-down"));

                this.ActionsData.Add(new ActionData(StringResource.admin_PagesPropertiesPreview, "Page", "Main", new { id = page.Id }, "glyphicon glyphicon-eye-open"));
                this.ActionsData.Add(new ActionData(StringResource.admin_PagesPropertiesEdit, this.GetActionForEditPage(contentType), "Admin", new { id = page.Id }, "glyphicon glyphicon-edit"));

                if (contentType == ContentType.Category)
                {
                    this.ActionsData.Add(new ActionData(StringResource.admin_PagesPropertiesActionEditProducts, "EditProducts", "Admin", new { id = page.Id }, "glyphicon glyphicon-list-alt"));
                }

                this.ActionsData.Add(new ActionData(StringResource.admin_PagesPropertiesDelete, "DeletePage", "Admin", new { id = page.Id }, "glyphicon glyphicon-trash"));
            }
        }

        private string GetActionForEditPage(ContentType type)
        {
            switch(type)
            {
                case ContentType.Page:
                    return "Page";
                case ContentType.Category:
                    return "Category";
            }

            return "Page";
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
