using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }

        public int Count { get; set; }

        public int MaxPages { get; set; }

        public string Id { get; set; }
    }
}