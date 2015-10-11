using InstantStore.WebUI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class ReferrerButtonViewModel
    {
        public ReferrerButtonViewModel(string url)
        {
            this.Url = url;
            this.Title = StringResource.Cancel;
            this.Classes = "btn btn-default btn-lg";
        }

        public ReferrerButtonViewModel(string url, string title)
        {
            this.Url = url;
            this.Title = title;
            this.Classes = "btn btn-default btn-lg";
        }

        public string Title { get; set; }
        
        public string Url { get; set; }

        public string Classes { get; set; }
    }
}