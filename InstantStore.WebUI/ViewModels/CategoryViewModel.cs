using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Resources;
using System.ComponentModel.DataAnnotations;
using InstantStore.Domain.Entities;

namespace InstantStore.WebUI.ViewModels
{
    public class CategoryViewModel
    {
        public CategoryViewModel()
        {
            this.Content = new PageViewModel();
            this.Initialize();
        }

        public CategoryViewModel(IRepository repository, Guid id)
            : this(new PageViewModel(repository, id), repository)
        {
        }

        public CategoryViewModel(PageViewModel pageViewModel, IRepository repository)
        {
            this.Content = pageViewModel;

            if (this.Content.ContentPage == null || !this.Content.ContentPage.IsCategory())
            {
                throw new InvalidOperationException("The entity is not category.");
            }

            var category = repository.GetCategoryById(this.Content.ContentPage.CategoryId.Value);
            if (category == null)
            {
                throw new Exception("Data is not consistent.");
            }

            this.ListType = category.ListType;
            this.Initialize(this.ListType == 2);
            this.CategoryImage = category.ImageId;
            this.IsImportant = category.IsImportant;
        }

        [Display(ResourceType = typeof(StringResource), Name = "admin_CategoryListTypeLabel")]
        public int ListType { get; set; }

        public PageViewModel Content { get; set; }

        public Guid? CategoryImage { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_CategoryIsImportant")]
        public bool IsImportant { get; set; }

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