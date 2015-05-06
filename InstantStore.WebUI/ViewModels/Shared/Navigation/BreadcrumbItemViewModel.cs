using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class BreadcrumbItemViewModel
    {
        public string Name { get; set; }
    
        public bool IsActive { get; set; }

        public NavigationLink Link { get; set; }
    }
}