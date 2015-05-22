using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public class ListViewModel
    {
        public IDictionary<string, IList<ListItemViewModel>> Groups { get; set; }

        public PaginationViewModel PageInfo { get; set; }
    }

    public class ListItemViewModel : BreadcrumbItemViewModel
    {
    }
}