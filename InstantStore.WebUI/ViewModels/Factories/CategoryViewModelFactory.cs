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
using InstantStore.WebUI.Models.Helpers;

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

        public static TilesViewModel GetPriorityCategories(IRepository repository, HttpRequestBase request)
        {
            var topCategories = repository.GetPriorityCategories();
            if (topCategories.Count == 0)
            {
                return null;
            }

            var defaultGroup = new List<TileViewModel>();

            var tilesViewModel = new TilesViewModel() 
            { 
                TileGroups = new List<TilesViewModelGroup> 
                { 
                    new TilesViewModelGroup(null, defaultGroup)
                } 
            };

            foreach(var categoryPage in topCategories)
            {
                // Extracting the page id. 
                var page = repository.GetPageByCategoryId(categoryPage.VersionId);
                if (page == null)
                {
                    // Log inconsistent items.
                    const string header = "[CategoryViewModelFactory:GetPriorityCategories],Warning,";
                    const string message = "Model inconsistent: category item doesn't have corresponding page. Category version id: ";
                    repository.LogErrorMessage(string.Concat(header, message, categoryPage.VersionId.ToString()), request);

                    continue;
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
                        var categoryPage = candidate.ContentPage;
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

            // Perform grouping and sort inside each group

            var exchangeRates = this.context.ExchangeRates.ToList();

            foreach (var group in products.GroupBy(x => string.Concat((x.Item2 ?? Guid.Empty).ToString(), "|", x.Item1), y => y.Item3))
            {
                var productList = group.ToList();
                //productList.Sort(this.Compare);

                var dataList = new List<Tuple<Product, CurrencyString>>();

                foreach (var product in productList)
                {
                    var price = user != null && product.Currency != null
                        ? (
                            !user.IsAdmin
                            ? new CurrencyString(product.GetPriceForUser(user, exchangeRates), currency.Text)
                            : product.PriceValueCash != null ? new CurrencyString(product.PriceValueCash.Value, product.Currency.Text) : null
                        )
                        : null;

                    dataList.Add(new Tuple<Product, CurrencyString>(product, price));
                    //this.AddItem(product, price, group.Key);
                }

                this.AddItems(group.Key, dataList);
            }

            return this.CreateViewModel(pagination);
        }

        //protected abstract void AddItem(Product product, CurrencyString price, string groupKey);
        protected abstract void AddItems(string groupKey, List<Tuple<Product, CurrencyString>> products);

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
        private Dictionary<Guid, Tuple<string, IList<TileViewModel>>> Items = new Dictionary<Guid, Tuple<string, IList<TileViewModel>>>();

        public TileProductViewModelFactory(InstantStoreDataContext context, User user, Guid categoryId, ProductActionData productActionData, int count, int offset)
            : base(context, user, categoryId, productActionData, count, offset)
        {
        }

        protected override object CreateViewModel(PaginationViewModel pagination)
        {
            var defaultGroup = this.Items.FirstOrDefault(x => x.Key == Guid.Empty);
            this.Items.Remove(Guid.Empty);

            var tileGroups = this.Items.Select(
                g => new TilesViewModelGroup(
                    new TileGroupKey
                    {
                        Link = new NavigationLink("Page", "Main") 
                        {
                            PageId = g.Key,
                            Text = g.Value.Item1
                        }
                    },
                    g.Value.Item2)
                ).ToList();

            if (defaultGroup.Value != null)
            {
                tileGroups.Insert(0, new TilesViewModelGroup(new TileGroupKey { Title = defaultGroup.Value.Item1 }, defaultGroup.Value.Item2));
            }

            return new TilesViewModel 
            { 
                Pagination = pagination,
                TileGroups = tileGroups
            };
        }

        protected override void AddItems(string groupKey, List<Tuple<Product, CurrencyString>> products)
        {
            var groupKeyData = groupKey.Split('|');
            var groupName = groupKeyData[1];
            var groupId = Guid.Parse(groupKeyData[0]);

            var tileViewModelList = products.Select(x => this.CreateItem(x.Item1, x.Item2)).ToList();
            this.Items.Add(groupId, new Tuple<string, IList<TileViewModel>>(groupName, tileViewModelList));
        }

        private TileViewModel CreateItem(Product product, CurrencyString price)
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

                /*
                if (!user.IsAdmin)
                {
                    tileViewModel.Action = this.CreateAddToCartLink(product);
                }
                */
            }

            return tileViewModel;
        }
    }

    public class ListProductViewModelFactory : ProductViewModelFactoryBase, IProductViewModelFactory
    {
        private Dictionary<Guid, Tuple<string, List<TableRowViewModel>>> Items = new Dictionary<Guid, Tuple<string, List<TableRowViewModel>>>();

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

            Tuple<string, List<TableRowViewModel>> value;
            var rowList = this.Items.TryGetValue(Guid.Empty, out value) ? value.Item2 : new List<TableRowViewModel>();

            foreach(var group in this.Items.Where(x => x.Key != Guid.Empty))
            {
                /*
                var groupAction = new NavigationLink("Page", "Main")
                {
                    PageId = group.Key,
                    Text = group.Value.Item1
                };
                */
                rowList.Add(new TableRowViewModel
                {
                    GroupCell = new TableCellViewModel(group.Value.Item1),
                    Id = group.Key.ToString()
                });

                rowList.AddRange(group.Value.Item2);
            }

            return new TableViewModel
            {
                Pagination = pagination,
                Header = headers,
                Rows = rowList,
                RowClickAction = this.productActionData != null ? new NavigationLink(this.productActionData.Action, this.productActionData.Controller) : null
            };
        }

        protected override void AddItems(string groupKey, List<Tuple<Product, CurrencyString>> products)
        {
            var groupKeyData = groupKey.Split('|');
            var groupName = groupKeyData[1];
            var groupId = Guid.Parse(groupKeyData[0]);

            var groupTableRows = products.Select(x => this.CreateItem(x.Item1, x.Item2)).ToList();
            this.Items.Add(groupId, new Tuple<string,List<TableRowViewModel>>(groupName, groupTableRows));
        }

        private TableRowViewModel CreateItem(Product product, CurrencyString price)
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
                    //user.IsAdmin ? new TableCellViewModel(string.Empty) : new TableCellViewModel(this.CreateAddToCartLink(product))
                    new TableCellViewModel(string.Empty)
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

            return new TableRowViewModel
            {
                Cells = cells,
                Id = product.VersionId.ToString(),
                ParentId = actionLink != null && actionLink.ParentId != null ? actionLink.ParentId.ToString() : null
            };
        }
    }
}