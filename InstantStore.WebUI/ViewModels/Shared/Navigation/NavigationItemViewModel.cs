using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class NavigationItemViewModel : BreadcrumbItemViewModel
    {
        public string Badge { get; set; }

        public string Glyph { get; set; }

        public Guid? ImageThumbnailId { get; set; }
    }
}