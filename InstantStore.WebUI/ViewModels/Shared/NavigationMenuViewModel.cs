using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class NavigationMenuViewModel
    {
        public string Title { get; set; }

        public string BackActionName { get; set; }

        public IList<NavigationItemViewModel> Items { get; set; }
    }
}