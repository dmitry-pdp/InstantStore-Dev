using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class ImageThumbnailViewModel
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public Guid ThumbnailId { get; set; }

        public NavigationLink Click { get; set; }
    }
}