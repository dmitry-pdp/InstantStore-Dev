using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InstantStore.WebUI.ViewModels
{
    public class NavigationLink
    {
        public string ActionName { get; set; }

        public string ControllerName { get; set; }

        public Guid PageId { get; set; }

        public string GetUrl(UrlHelper helper)
        {
            string url = null;
            if (this.ControllerName != null)
            {
                url = helper.Action(this.ActionName, this.ControllerName);
            }
            else if (this.ActionName != null)
            {
                url = helper.Action(this.ActionName);
            }
            else
            {
                url = helper.Action("Page", new { id = PageId });
            }

            return url;
        }
    }
}