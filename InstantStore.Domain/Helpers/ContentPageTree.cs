using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantStore.Domain.Helpers
{
    public interface IPageContentProvider
    {
        List<ContentPage> GetChildren(Guid parentId);

        ContentPage GetPage(Guid id);

        void TrashPage(Guid id);
    }

    public class RepositoryPageContentProvider : IPageContentProvider
    {
        private IRepository repository;

        public RepositoryPageContentProvider(IRepository repository)
        {
            this.repository = repository;
        }

        public List<ContentPage> GetChildren(Guid parentId)
        {
            return this.repository.GetPages(parentId, null).ToList();
        }

        public ContentPage GetPage(Guid id)
        {
            return this.repository.GetPageById(id);
        }

        public void TrashPage(Guid id)
        {
            this.repository.TrashPage(id);
        }
    }

    public class ContentPageTree
    {
        private IPageContentProvider provider;

        private IDictionary<Guid, ContentPageTreeItem> cache = new Dictionary<Guid, ContentPageTreeItem>();

        public ContentPageTree(Guid? parent, IPageContentProvider provider)
        { 
            this.provider = provider;
            var content = parent != null ? provider.GetPage(parent.Value) : new ContentPage() { Id = Guid.Empty, Name = "Домашняя страница" };
            this.Root = new ContentPageTreeItem(content, null);
            this.GerenateForParent(this.Root);
        }

        public ContentPageTreeItem Root { get; private set; }

        public ContentPageTreeItem LookupPage(Guid id, ContentPageTreeItem current)
        {
            if (current.Page.Id == id)
            {
                return current;
            }

            foreach (var child in current.Children)
            {
                var result = LookupPage(id, child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void GerenateForParent(ContentPageTreeItem parent)
        {
            var children = this.provider.GetChildren(parent.Page.Id);
            if (children == null || !children.Any())
            {
                return;
            }

            foreach (var child in children)
            {
                if (!this.cache.ContainsKey(child.Id))
                {
                    var item = new ContentPageTreeItem(child, parent);
                    parent.Children.Add(item);
                    this.cache.Add(child.Id, item);
                }
                else
                {
                    // Cyclic dependecies has been encountered.
                    // Fixing the database by assigning the page in to fake root.
                    this.provider.TrashPage(child.Id);
                }
            }

            foreach (var child in parent.Children)
            {
                this.GerenateForParent(child);
            }
        }
    }

    public class ContentPageTreeItem
    {
        public ContentPageTreeItem(ContentPage page, ContentPageTreeItem parent)
        {
            this.Children = new List<ContentPageTreeItem>();
            this.Page = page;
            this.Parent = parent;
        }

        public ContentPageTreeItem Parent { get; private set; }

        public IList<ContentPageTreeItem> Children { get; private set; }

        public ContentPage Page { get; private set; }
    }
}
