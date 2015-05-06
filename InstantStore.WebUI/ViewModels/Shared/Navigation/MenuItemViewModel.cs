using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class MenuItemViewModel : NavigationItemViewModel
    {
        public int Level { get; set; }

        public IList<MenuItemViewModel> Items { get; set; }
    }
}