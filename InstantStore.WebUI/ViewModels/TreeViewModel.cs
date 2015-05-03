using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Resources;
using InstantStore.Domain.Entities;

namespace InstantStore.WebUI.ViewModels
{
    public class TreeViewModel
    {
        public string TreeId { get; set; }

        public CategoryTreeItemViewModel Root { get; set; }
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
            this.Type = (ContentType)page.ContentType;
        }

        public static CategoryTreeItemViewModel CreateNavigationTree(IRepository repository, bool excludeProducts = true)
        {
            var root = new CategoryTreeItemViewModel { Name = StringResource.admin_PageTreeRoot, Id = Guid.Empty };
            root.InitializeSubCategories(repository, excludeProducts);
            return root;
        }

        private void InitializeSubCategories(IRepository repository, bool excludeProducts = true)
        {
            Func<ContentPage, bool> filter = excludeProducts ? p => p.ContentType != (int)ContentType.Product : (Func<ContentPage, bool>)null;
            this.Items = GetPages(repository, this.Id == Guid.Empty ? (Guid?)null : this.Id, filter);
            this.Items.ForEach(x => x.InitializeSubCategories(repository, excludeProducts));
        }

        private static List<CategoryTreeItemViewModel> GetPages(IRepository repository, Guid? parentId, Func<ContentPage, bool> filter)
        {
            return repository.GetPages(parentId, filter).Select(p => new CategoryTreeItemViewModel(p)).OrderBy(y => y.Position).ToList();
        }

        public string Name { get; set; }

        public Guid Id { get; set; }

        public uint Position { get; set; }

        public ContentType Type { get; set; }

        public List<CategoryTreeItemViewModel> Items { get; private set; }
    }
}