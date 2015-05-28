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
            : this(repository.GetPageById(id), true)
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
            this.ShowInMenu = contentPage.ShowInMenu;
            this.ContentPage = contentPage;
        }

        public Guid Id { get; set; } 

        [Display(ResourceType = typeof(StringResource), Name = "admin_Name")]
        [Required]
        public string Name { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_PageContent")]
        public string Text { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_CategoryShowInMenuLabel")]
        public bool ShowInMenu { get; set; }

        [Required]
        public Guid ParentCategoryId { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_PageContent")]
        public AttachmentViewModel Attachment { get; set; }

        public CategoryTreeItemViewModel RootCategory { get; private set; }

        public ContentPage ContentPage { get; private set; }

        public static dynamic CreateTreeNode(CategoryTreeItemViewModel node, bool useIcons = true)
        {
            var icon = useIcons 
                ? (node.Id == Guid.Empty ? "glyphicon glyphicon-home" : (node.IsCategory ? "glyphicon glyphicon-folder-open" : "glyphicon glyphicon-file"))
                : null;
            var subItems = node.Items != null ? node.Items.Select(i => CreateTreeNode(i)).ToArray() : null;
            return new 
            { 
                text = node.Name, 
                nodes = subItems != null && subItems.Any() ? subItems : null, 
                id = node.Id.ToString(), 
                icon = icon,
                key = node.Key
            };
        }
     }
}