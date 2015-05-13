using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InstantStore.WebUI.ViewModels
{
    public class NavigationLink
    {
        public string Text { get; set; }

        public string ActionName { get; set; }

        public string ControllerName { get; set; }

        public Guid? PageId { get; set; }

        public Guid? ParentId { get; set; }

        public string GetUrl(UrlHelper helper)
        {
            object parameters = null;
            if (this.PageId != null)
            {
                parameters = this.ParentId == null ? (object)new { id = this.PageId } : (object)new { id = PageId, parentId = this.ParentId };
            }

            string action = this.ActionName ?? "Page";
            
            string url = null;
            if (this.ControllerName != null)
            {
                url = helper.Action(this.ActionName, this.ControllerName, parameters);
            }
            else 
            {
                url = helper.Action(action, parameters);
            }

            return url;
        }
    }
}