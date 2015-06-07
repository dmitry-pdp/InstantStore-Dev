using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.Domain.Helpers;
using InstantStore.WebUI.Resources;
using InstantStore.WebUI.ViewModels.Helpers;

namespace InstantStore.WebUI.ViewModels.Factories
{
    public enum ListingViewProductSettings
    { 
        /* Product view for non logged in user */
        Default,

        /* Product view for logged in user. */
        User,

        /* Product view for admin */
        Admin,

        /* Product view for admin */
        AdminSettings,

        /* Product view for copy to another category form */
        AdminMove,

        /* Product view for delete products from category form */
        AdminDelete
    }

    public static class CategoryViewModelFactory
    {
        private static NumberFormatInfo russianCultureNumber = new CultureInfo("ru-RU").NumberFormat;

        public static TilesViewModel GetPriorityCategories(IRepository repository)
        {
            var topCategories = repository.GetPriorityCategories();
            if (topCategories.Count == 0)
            {
                return null;
            }

            var defaultGroup = new List<TileViewModel>();

            var tilesViewModel = new TilesViewModel() 
            { 
                TileGroups = new List<KeyValuePair<string, IList<TileViewModel>>> 
                { 
                    new KeyValuePair<string, IList<TileViewModel>>(string.Empty, defaultGroup)
                } 
            };

            foreach(var categoryPage in topCategories)
            {
                var page = repository.GetPageByCategoryId(categoryPage.VersionId);
                if (page == null)
                {
                    throw new ApplicationException("Model.Inconsistent");
                }

                defaultGroup.Add(new TileViewModel
                {
                    Link = new NavigationLink { PageId = page.Id },
                    Name = categoryPage.Name,
                    ImageId = categoryPage.ImageId ?? Guid.Empty
                });
            }

            return tilesViewModel;
        }

        public static MediaListViewModel CreatePopularProducts(this IRepository repository, Guid? pageId)
        {
            var viewModel = new MediaListViewModel();
            viewModel.Title = StringResource.NavBar_PopularProducts;
            viewModel.Items = repository.GetProductsByPopularity(3).Select(product => new MediaItemViewModel
            {
                Name = product.Name,
                Link = new NavigationLink { PageId = product.VersionId, ActionName = "Page", ControllerName = "Main" },
                ImageThumbnailId = product.MainImageId
            })
            .ToList();

            return viewModel;
        }

        public static MediaListViewModel CreateSimilarProducts(this IRepository repository, Guid? pageId)
        {
            var viewModel = new MediaListViewModel();
            viewModel.Title = StringResource.NavBar_PopularProducts;
            viewModel.Items = repository.GetProductsByPopularity(3).Select(product => new MediaItemViewModel
            {
                Name = product.Name,
                Link = new NavigationLink { PageId = product.VersionId },
                ImageThumbnailId = product.MainImageId
            })
            .ToList();

            return viewModel;
        }

        public static object CreateCategoryViewModel(
            User user, 
            ContentPage categoryPage, 
            int count, 
            int offset, 
            ListingViewProductSettings displayHint)
        {
            if (categoryPage == null || !categoryPage.IsCategory())
            {
                return null;
            }

            using (var context = new InstantStoreDataContext())
            {
                var category = context.Categories.FirstOrDefault(x => x.VersionId == categoryPage.CategoryId.Value);
                if (category == null)
                {
                    throw new ArgumentException("Category does not exist for id.");
                }

                var productActionData = user != null && user.IsAdmin && displayHint == ListingViewProductSettings.AdminSettings
                    ? new ProductActionData("Admin", "Product", categoryPage.Id)
                    : new ProductActionData("Main", "Page", categoryPage.Id);

                var factory = category.ListType == 1
                    ? (IProductViewModelFactory)new ListProductViewModelFactory(context, user, categoryPage.Id, productActionData, count, offset)
                    : (IProductViewModelFactory)new TileProductViewModelFactory(context, user, categoryPage.Id, productActionData, count, offset);

                return factory.CreateProductViewModel();
            }
        }
    }

    public class ProductActionData 
    {
        private Guid? parentId;

        public ProductActionData(string controller, string action)
        {
            this.Controller = controller;
            this.Action = action;
        }

        public ProductActionData(string controller, string action, Guid? parentId)
        {
            this.Controller = controller;
            this.Action = action;
            this.parentId = parentId;
        }

        public string Controller { get; private set; }

        public string Action { get; private set; }

        public NavigationLink CreateActionLink(Product product)
        {
            var parent = this.parentId;

            if (product != null && 
                !product.ProductToCategories.Any(x => x.CategoryId == this.parentId && x.ProductId == product.VersionId && x.GroupId == null))
            {
                var categoryCandidates = product.ProductToCategories.Where(x => x.ProductId == product.VersionId && x.GroupId == null).ToList();
                if (categoryCandidates.Count == 1)
                {
                    parent = categoryCandidates[0].CategoryId;
                }
                else 
                {
                    foreach (var candidate in categoryCandidates)
                    {
                        var categoryPage = candidate.Category;
                        while(categoryPage != null && categoryPage.Id != this.parentId)
                        {
                            categoryPage = categoryPage.Parent;
                        }

                        if (categoryPage != null)
                        {
                            parent = candidate.CategoryId;
                            break;
                        }
                    }
                }
            }

            return new NavigationLink(this.Action, this.Controller) 
            { 
                ParentId = parent, 
                PageId = product != null ? product.VersionId : (Guid?)null 
            };
        }
    }

    public interface IProductViewModelFactory
    {
        object CreateProductViewModel();
    }

    public abstract class ProductViewModelFactoryBase: IProductViewModelFactory
    {
        protected InstantStoreDataContext context;
        protected ProductActionData productActionData;
        protected User user;
        protected Guid categoryId;
        protected int count;
        protected int offset;

        public ProductViewModelFactoryBase(
            InstantStoreDataContext context, 
            User user, 
            Guid categoryId,
            ProductActionData productActionFactory, 
            int count, 
            int offset)
        {
            this.productActionData = productActionFactory;
            this.context = context;
            this.user = user;
            this.categoryId = categoryId;
            this.count = count;
            this.offset = offset;
        }

        public virtual object CreateProductViewModel()
        {
            var products = this.context.GetProductsForCategory(this.categoryId, this.offset, this.count);
            if (products == null)
            {
                return null;
            }

            var maxProducts = this.context.GetProductsCountForCategory(this.categoryId);
            var maxPages = maxProducts / this.count;
            var pagination = new PaginationViewModel
            {
                MaxPages = maxPages,
                CurrentPage = maxProducts != 0 ? this.offset * maxPages / maxProducts : 0,
                Count = this.count
            };

            var currency = this.user != null && this.user.DefaultCurrencyId != null && !this.user.IsAdmin
                ? this.context.Currencies.FirstOrDefault(x => x.Id == this.user.DefaultCurrencyId)
                : null;

            var exchangeRates = this.user != null && this.user.DefaultCurrencyId != null
                ? this.context.ExchangeRates.Where(x => x.ToCurrencyId == this.user.DefaultCurrencyId).ToList()
                : new List<ExchangeRate>();

            // Perform grouping and sort inside each group

            foreach(var group in products.GroupBy(x => x.Key))
            {
                var productList = group.Select(x => x.Value).ToList();
                productList.Sort(this.Compare);

                foreach (var product in productList)
                {
                    this.AddItem(
                        product, (user != null && currency != null)
                            ? new CurrencyString(product.GetPriceForUser(user, this.context.ExchangeRates), currency.Text)
                            : null,
                        group.Key);
                }
            }

            return this.CreateViewModel(pagination);
        }

        protected abstract void AddItem(Product product, CurrencyString price, string groupName);

        protected abstract object CreateViewModel(PaginationViewModel pagination);

        protected string GetProductAvailableString(Product product)
        {
            return product.IsAvailable ? StringResource.Yes : StringResource.No;
        }

        protected NavigationLink CreateAddToCartLink(Product product)
        {
            return new NavigationLink 
            { 
                ControllerName = "User", 
                ActionName = "AddToCart", 
                PageId = product.VersionId, 
                Text = StringResource.productTile_AddToCart 
            };
        }

        protected NavigationLink CreateProductActionLink(Product product)
        {
            return this.productActionData != null ? this.productActionData.CreateActionLink(product) : null;
        }

        private int Compare(Product x, Product y)
        {
            return string.Compare(x.Name, y.Name);
        }
    }

    public class TileProductViewModelFactory : ProductViewModelFactoryBase
    {
        private IList<KeyValuePair<string, IList<TileViewModel>>> Items = new List<KeyValuePair<string, IList<TileViewModel>>>();

        public TileProductViewModelFactory(InstantStoreDataContext context, User user, Guid categoryId, ProductActionData productActionData, int count, int offset)
            : base(context, user, categoryId, productActionData, count, offset)
        {
        }

        protected override object CreateViewModel(PaginationViewModel pagination)
        {
            var defaultGroup = this.Items.FirstOrDefault(x => x.Key == null);
            this.Items.Remove(defaultGroup);
            this.Items.Insert(0, defaultGroup);

            return new TilesViewModel 
            { 
                Pagination = pagination, 
                TileGroups = this.Items
            };
        }

        protected override void AddItem(Product product, CurrencyString price, string groupName)
        {
            var tileViewModel = new TileViewModel
            {
                Link = this.CreateProductActionLink(product),
                ImageId = product.MainImageId ?? Guid.Empty,
                Name = product.Name
            };

            if (this.user != null)
            {
                var attributes = new AttributeList();
                attributes.Add(new KeyValuePair<string, string>(StringResource.productTile_Available, this.GetProductAvailableString(product)));

                if (price != null)
                {
                    attributes.Add(new KeyValuePair<string, string>(StringResource.productTile_Price, price.ToString()));
                }
                
                tileViewModel.Attributes = attributes;

                if (!user.IsAdmin)
                {
                    tileViewModel.Action = this.CreateAddToCartLink(product);
                }
            }

            if (this.Items.Any(x => x.Key == groupName))
            {
                this.Items.First(x => x.Key == groupName).Value.Add(tileViewModel);
            }
            else
            {
                this.Items.Add(new KeyValuePair<string, IList<TileViewModel>>(groupName, new List<TileViewModel> { tileViewModel }));
            }
        }
    }

    public class ListProductViewModelFactory : ProductViewModelFactoryBase, IProductViewModelFactory
    {
        private IList<KeyValuePair<string, List<TableRowViewModel>>> Items = new List<KeyValuePair<string, List<TableRowViewModel>>>();

        public ListProductViewModelFactory(InstantStoreDataContext context, User user, Guid categoryId, ProductActionData productActionData, int count, int offset)
            : base(context, user, categoryId, productActionData, count, offset)
        {
        }

        protected override object CreateViewModel(PaginationViewModel pagination)
        {
            var headers = user != null 
                ? new List<TableCellViewModel>
                {
                    new TableCellViewModel(string.Empty),
                    new TableCellViewModel(StringResource.admin_Name),
                    new TableCellViewModel(StringResource.productTile_Available),
                    new TableCellViewModel(StringResource.productTile_Price),
                    new TableCellViewModel(string.Empty), // Add to cart action column
                }
                : new List<TableCellViewModel>
                {
                    new TableCellViewModel(string.Empty),
                    new TableCellViewModel(StringResource.admin_Name)
                };

            var rowList = this.Items.Any(x => x.Key == null) ? this.Items.First(x => x.Key == null).Value : new List<TableRowViewModel>();
            foreach(var group in this.Items.Where(x => x.Key != null))
            {
                rowList.Add(new TableRowViewModel
                {
                    GroupCell = new TableCellViewModel(group.Key)
                });

                rowList.AddRange(group.Value);
            }

            return new TableViewModel
            {
                Pagination = pagination,
                Header = headers,
                Rows = rowList,
                RowClickAction = this.productActionData != null ? new NavigationLink(this.productActionData.Action, this.productActionData.Controller) : null
            };
        }

        protected override void AddItem(Product product, CurrencyString price, string groupName)
        {
            var cells = user != null
                ? new List<TableCellViewModel>
                {
                    new TableCellViewModel(new ImageThumbnailViewModel{
                        ThumbnailId = product.MainImageId ?? Guid.Empty,
                        Width = 64
                    }),
                    new TableCellViewModel(product.Name),
                    new TableCellViewModel(this.GetProductAvailableString(product)),
                    new TableCellViewModel(price != null ? price.ToString() : StringResource.NotAvailable),
                    user.IsAdmin ? new TableCellViewModel(string.Empty) : new TableCellViewModel(this.CreateAddToCartLink(product))
                }
                : new List<TableCellViewModel>
                {
                    new TableCellViewModel(new ImageThumbnailViewModel{
                        ThumbnailId = product.MainImageId ?? Guid.Empty,
                        Width = 64
                    }),
                    new TableCellViewModel(product.Name)
                };

            var actionLink = this.CreateProductActionLink(product);

            var row = new TableRowViewModel
            {
                Cells = cells,
                Id = product.VersionId.ToString(),
                ParentId = actionLink != null && actionLink.ParentId != null ? actionLink.ParentId.ToString() : null
            };

            if (this.Items.Any(x => x.Key == groupName))
            {
                this.Items.First(x => x.Key == groupName).Value.Add(row);
            }
            else
            {
                this.Items.Add(new KeyValuePair<string, List<TableRowViewModel>>(groupName, new List<TableRowViewModel> { row }));
            }
        }
    }
}