using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class DialogViewModel
    {
        public string DialogId { get; set; }

        public string DialogBodyId { get; set; }

        public string DialogSubmitId { get; set; }
        
        public string Title { get; set; }

        public string ButtonText { get; set; }

        public bool ShowCancelButton { get; set; }
    }
}