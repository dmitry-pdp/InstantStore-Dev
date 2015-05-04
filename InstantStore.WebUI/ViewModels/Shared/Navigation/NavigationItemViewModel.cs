using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class NavigationItemViewModel
    {
        public string Name { get; set; }

        public string Badge { get; set; }

        public bool IsActive { get; set; }

        public NavigationLink Link { get; set; }
    }
}