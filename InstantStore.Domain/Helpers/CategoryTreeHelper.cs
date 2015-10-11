using System;
using System.Linq;

using InstantStore.Domain.Concrete;
using InstantStore.Domain.Exceptions;

namespace InstantStore.Domain.Helpers
{
    public static class CategoryTreeBuilder
    {
        public static void RebuidCategoryTreeGroups(InstantStoreDataContext context, Guid updatedPageId)
        {
            Guid parentPageId = updatedPageId;

            // Walking up the tree rebuilding the group we just came from.
            while (parentPageId != Guid.Empty)
            { 
                parentPageId = UpdateCategoryGropus(parentPageId, context) ?? Guid.Empty;                
            }
        }

        /// <summary>
        /// Updates groups for the given category.
        /// </summary>
        /// <remarks>
        /// Assumes that child groups are valid.
        /// </remarks>
        public static Guid? UpdateCategoryGropus(Guid categoryId, InstantStoreDataContext context)
        { 
            var categoryPage = context.ContentPages.FirstOrDefault(x => x.Id == categoryId);
            if (categoryPage == null)
            {
                return categoryId;
            }

            // If the page is not category then return.
            if (!categoryPage.IsCategory())
            {
                return categoryPage.ParentId;
            }

            // Extract all existing groups
            var categoryGroups = context.ProductToCategories.Where(x => x.CategoryId == categoryId).ToList();
            var categoryChildren = context.ContentPages.Where(x => x.ParentId != null && x.ParentId == categoryId).ToList().Where(x => x.IsCategory());
            
            // Extracting the category children.
            foreach (var childPage in categoryChildren)
            {
                // All products from the real child category
                var childProducts = context.ProductToCategories.Where(x => x.CategoryId == childPage.Id).ToList();
                
                // Products of the child group of the current category.
                var childGroup = categoryGroups.Where(x => x.GroupId == childPage.Id).ToList();

                // Delete all products which are not exist in the child anymore.
                var productsToDeleteInGroup = childGroup.Where(x => !childProducts.Any(y => y.ProductId == x.ProductId)).ToList();
                context.ProductToCategories.DeleteAllOnSubmit(productsToDeleteInGroup);

                // Insert all products which are missing in the category group.
                var productsToInsertInGroup = childProducts.Where(x => !childGroup.Any(y => y.ProductId == x.ProductId)).ToList();
                context.ProductToCategories.InsertAllOnSubmit(productsToInsertInGroup.Select(x => new ProductToCategory
                {
                    Id = Guid.NewGuid(),
                    CategoryId = categoryId,
                    GroupId = childPage.Id,
                    ProductId = x.ProductId,
                    UpdateTime = DateTime.Now
                }));
            }         
       
            // Removing groups which are no longer children
            var noChildGroup = categoryGroups.Where(x => x.GroupId != null && !categoryChildren.Any(y => y.Id == x.GroupId));
            context.ProductToCategories.DeleteAllOnSubmit(noChildGroup);
            
            // Update all the gropus.
            context.SubmitChanges();

            // Return category's parent id.
            return categoryPage.ParentId;
        }
    }
}
