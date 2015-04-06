using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Resources;
using System.ComponentModel.DataAnnotations;

namespace InstantStore.WebUI.ViewModels
{
    public class CategoryViewModel : PageViewModel
    {
        public CategoryViewModel()
        {
            this.Initialize();
        }

        [Display(ResourceType = typeof(StringResource), Name = "admin_CategoryListTypeLabel")]
        public int ListType { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_CategoryShowInMenuLabel")]
        public bool ShowInMenu { get; set; }

        public List<SelectListItem> ListTypes { get; private set; }

        public void Initialize()
        {
            this.ListTypes = new List<SelectListItem>
            {
                new SelectListItem{
                    Text = StringResource.admin_CategoryTypeList,
                    Value = "1",
                    Selected = false
                },
                new SelectListItem{
                    Text = StringResource.admin_CategoryTypeTiles,
                    Value = "2",
                    Selected = true
                }
            };
        }
    }
}