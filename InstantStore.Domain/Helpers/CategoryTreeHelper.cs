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
            // Checkin input arguments.

            if (updatedPageId == Guid.Empty)
            {
                return;
            }
            
            // Here we are assuming that page is the category and already assigned the product with group set to null.

            var currentPage = context.ContentPages.FirstOrDefault(x => x.Id == updatedPageId);
            if (currentPage == null)
            {
                throw new ModelValidationException("No page found matching the id.");
            }

            if (!currentPage.IsCategory() || currentPage.Category == null)
            {
                throw new ModelValidationException("Page for updated category is not a category page.");
            }

            var currentCategoryProducts = context.ProductToCategories.Where(x => x.CategoryId == currentPage.Id);
            if (!currentCategoryProducts.Any(x => x.GroupName == null))
            { 
                throw new ModelValidationException("Updated category does not contains ungroupped products.");
            }

            // Walking up the tree rebuilding the group we just came from.

            Guid parentPageId = currentPage.ParentId ?? Guid.Empty;
            Guid currentPageId = currentPage.Id;

            while (parentPageId != Guid.Empty)
            { 
                // Get next parent category page in the tree.

                var parentPage = context.ContentPages.FirstOrDefault(x => x.Id == parentPageId);
                if (parentPage == null)
                {
                    throw new ModelValidationException(string.Format("Category tree is invalid. Parent page is not found for id: {0}", parentPageId));
                }

                parentPageId = parentPage.ParentId ?? Guid.Empty;

                // If parent page is not category continue walking up in the tree.
                
                if (!parentPage.IsCategory())
                {
                    continue;
                }

                // Here we found a parent category, so extracting the current category product group.

                var parentCategoryProductsForCurrentGroup = context.ProductToCategories
                    .Where(x => x.CategoryId == parentPage.Id && x.GroupName == currentPage.Name)
                    .ToList();

                // Getting the products gropup in the current category and updating the parent category group.

                var currentCategoryProductGroups = context.ProductToCategories
                    .Where(x => x.CategoryId == currentPageId)
                    .ToList();

                var productsToAdd = currentCategoryProductGroups
                    .Where(x => !parentCategoryProductsForCurrentGroup.Any(y => y.ProductId == x.ProductId));

                var productsToDelete = parentCategoryProductsForCurrentGroup
                    .Where(x => !currentCategoryProductGroups.Any(y => y.ProductId == x.ProductId));

                context.ProductToCategories.DeleteAllOnSubmit(productsToDelete);

                context.ProductToCategories.InsertAllOnSubmit(productsToAdd.Select(x => new ProductToCategory
                {
                    Id = Guid.NewGuid(),
                    CategoryId = parentPage.Id,
                    GroupName = currentPage.Name,
                    ProductId = x.ProductId,
                    UpdateTime = DateTime.Now
                }));

                // Applying changes
                
                context.SubmitChanges();

                // Move up to the next node
                // Parent page id is already assigned.

                currentPage = parentPage;
                currentPageId = parentPage.Id;
            }
        }
    }
}
