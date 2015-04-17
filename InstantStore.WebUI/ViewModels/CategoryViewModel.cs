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

        public CategoryViewModel(IRepository repository, Guid id)
            : base(repository, id)
        {
            if (this.ContentPage.CategoryId != null)
            {
                var category = repository.GetCategoryById(this.ContentPage.CategoryId.Value);
                if (category == null)
                {
                    throw new Exception("Data is not consistent.");
                }

                this.ListType = category.ListType;
                this.Initialize(this.ListType == 2);
                this.CategoryImage = category.ImageId;
                this.ShowInMenu = category.ShowInMenu;
            }
        }

        [Display(ResourceType = typeof(StringResource), Name = "admin_CategoryListTypeLabel")]
        public int ListType { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_CategoryShowInMenuLabel")]
        public bool ShowInMenu { get; set; }

        public Guid? CategoryImage { get; set; }

        public List<SelectListItem> ListTypes { get; private set; }

        public void Initialize(bool isTiles = true)
        {
            this.ListTypes = new List<SelectListItem>
            {
                new SelectListItem{
                    Text = StringResource.admin_CategoryTypeList,
                    Value = "1",
                    Selected = !isTiles
                },
                new SelectListItem{
                    Text = StringResource.admin_CategoryTypeTiles,
                    Value = "2",
                    Selected = isTiles
                }
            };
        }
    }
}