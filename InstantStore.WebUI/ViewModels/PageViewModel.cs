using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Resources;

namespace InstantStore.WebUI.ViewModels
{
    public class PageViewModel
    {
        public PageViewModel()
        {
        }

        public PageViewModel(IRepository repository)
        {
            this.InitializeRootCategory(repository);
        }

        public PageViewModel(ContentPage page)
        {
            this.Id = page.Id;
            this.Name = page.Name;
            this.Text = page.Text;
        }

        public void InitializeRootCategory(IRepository repository)
        {
            this.RootCategory = new CategoryTreeItemViewModel { Name = StringResource.admin_PageTreeRoot, Id = Guid.Empty };
            this.RootCategory.Items.AddRange(this.GetCategoryListForParentId(repository, null));
        }

        public Guid Id { get; set; } 

        [Display(ResourceType = typeof(StringResource), Name = "admin_Name")]
        [Required]
        public string Name { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_PageContent")]
        public string Text { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_PageParent")]
        [Required]
        public string ParentCategory { get; set; }

        [Required]
        public Guid ParentCategoryId { get; set; }

        public CategoryTreeItemViewModel RootCategory { get; private set; }
    
        private List<CategoryTreeItemViewModel> GetCategoryListForParentId(IRepository repository, Guid? parentId)
        {
            var categories = repository.GetPages(parentId).Select(p => new CategoryTreeItemViewModel(p)).OrderBy(y => y.Position).ToList();
            categories.ForEach(x => x.InitializeSubCategories(repository));
            return categories;
        }
    }

    public class CategoryTreeItemViewModel
    {
        public CategoryTreeItemViewModel()
        {
            this.Items = new List<CategoryTreeItemViewModel>();
        }

        public CategoryTreeItemViewModel(ContentPage page)
        {
            this.Id = page.Id;
            this.Name = page.Name;
            this.Position = (uint)page.Position;
        }

        public void InitializeSubCategories(IRepository repository)
        {
            this.Items = repository.GetPages(this.Id).Select(x => new CategoryTreeItemViewModel(x)).OrderBy(y => y.Position).ToList();
            this.Items.ForEach(x => x.InitializeSubCategories(repository));
        }

        public string Name { get; set; }

        public Guid Id { get; set; }

        public uint Position { get; set; }

        public List<CategoryTreeItemViewModel> Items { get; private set; }
    }
}