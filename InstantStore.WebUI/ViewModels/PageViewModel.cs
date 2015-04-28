using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Resources;
using InstantStore.Domain.Entities;

namespace InstantStore.WebUI.ViewModels
{

    public class PageViewModel
    {
        public PageViewModel()
        {
        }

        public PageViewModel(IRepository repository, Guid id)
            : this(repository.GetPageById(id))
        {
        }

        public PageViewModel(ContentPage contentPage, bool canEdit)
            : this(contentPage)
        {
            if (this.Attachment != null)
            {
                this.Attachment.CanEdit = canEdit;
            }            
        }

        public PageViewModel(ContentPage contentPage)
        {
            if (contentPage == null)
            {
                throw new ArgumentNullException("contentPage");
            }

            this.Id = contentPage.Id;
            this.Name = contentPage.Name;
            this.Text = contentPage.Text;
            this.ParentCategoryId = contentPage.ParentId ?? Guid.Empty;
            this.Attachment = contentPage.AttachmentId != null ? new AttachmentViewModel(contentPage) : null;
            this.ContentPage = contentPage;
        }

        public Guid Id { get; set; } 

        [Display(ResourceType = typeof(StringResource), Name = "admin_Name")]
        [Required]
        public string Name { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_PageContent")]
        public string Text { get; set; }

        [Required]
        public Guid ParentCategoryId { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_PageContent")]
        public AttachmentViewModel Attachment { get; set; }

        public CategoryTreeItemViewModel RootCategory { get; private set; }

        public ContentPage ContentPage { get; private set; }

        public static dynamic CreateTreeNode(CategoryTreeItemViewModel node)
        {
            var icon = node.Type == ContentType.RootPage ? "glyphicon glyphicon-home" : (node.Type == ContentType.Category ? "glyphicon glyphicon-folder-open" : "glyphicon glyphicon-file");
            var subItems = node.Items.Select(i => CreateTreeNode(i)).ToArray();
            return new { text = node.Name, nodes = subItems.Any() ? subItems : null, id = node.Id.ToString(), icon = icon };
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