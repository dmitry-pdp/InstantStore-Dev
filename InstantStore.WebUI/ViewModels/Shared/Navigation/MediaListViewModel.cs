using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class MediaListViewModel
    {
        public string Title { get; set; }

        public IList<MediaItemViewModel> Items { get; set; }
    }
}