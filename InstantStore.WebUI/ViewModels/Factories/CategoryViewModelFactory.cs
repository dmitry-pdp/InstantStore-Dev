using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.Domain.Helpers;
using InstantStore.WebUI.Resources;
using System.Globalization;
using InstantStore.WebUI.ViewModels.Helpers;

namespace InstantStore.WebUI.ViewModels.Factories
{
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

            var tilesViewModel = new TilesViewModel() { Tiles = new List<TileViewModel>() };

            foreach(var categoryPage in topCategories)
            {
                var page = repository.GetPageByCategoryId(categoryPage.VersionId);
                if (page == null)
                {
                    throw new ApplicationException("Model.Inconsistent");
                }

                tilesViewModel.Tiles.Add(new TileViewModel {
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
                Link = new NavigationLink { PageId = product.VersionId },
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
    }

    public interface IProductViewModelFactory
    {
        object CreateProductViewModel();
    }

    public abstract class ProductViewModelFactoryBase: IProductViewModelFactory
    {
        protected IRepository repository;
        protected User user;
        protected Guid categoryId;
        protected int count;
        protected int offset;

        public ProductViewModelFactoryBase(IRepository repository, User user, Guid categoryId, int count, int offset)
        {
            this.repository = repository;
            this.user = user;
            this.categoryId = categoryId;
            this.count = count;
            this.offset = offset;
        }

        public virtual object CreateProductViewModel()
        {
            var products = this.repository.GetProductsForCategory(this.categoryId, this.offset, this.count);
            if (products == null)
            {
                return null;
            }

            var maxProducts = this.repository.GetProductsCountForCategory(this.categoryId);
            var maxPages = maxProducts / this.count;
            var pagination = new PaginationViewModel
            {
                MaxPages = maxPages,
                CurrentPage = maxProducts != 0 ? this.offset * maxPages / maxProducts : 0,
                Count = this.count
            };

            var currency = this.user != null && this.user.DefaultCurrencyId != null && !this.user.IsAdmin
                ? this.repository.GetCurrencies().FirstOrDefault(x => x.Id == this.user.DefaultCurrencyId)
                : null;

            var exchangeRates = this.user != null && this.user.DefaultCurrencyId != null
                ? this.repository.GetExchangeRates().Where(x => x.ToCurrencyId == this.user.DefaultCurrencyId)
                : new List<ExchangeRate>();

            foreach(var product in products)
            {
                this.AddItem(product, (user != null && currency != null && !user.IsAdmin)
                    ? new CurrencyString(product.GetPriceForUser(user, repository.GetExchangeRates()), currency.Text)
                    : null);
            }

            return this.CreateViewModel(pagination);
        }

        protected abstract void AddItem(Product product, CurrencyString price);

        protected abstract object CreateViewModel(PaginationViewModel pagination);

        protected string GetProductAvailableString(Product product)
        {
            return product.IsAvailable ? StringResource.Yes : StringResource.No;
        }

        protected NavigationLink CreateAddToCartLink(Product product)
        {
            return new NavigationLink 
            { 
                ControllerName = "Main", 
                ActionName = "AddToCart", 
                PageId = product.VersionId, 
                Text = StringResource.productTile_AddToCart 
            };
        }
    }

    public class TileProductViewModelFactory : ProductViewModelFactoryBase
    {
        private IList<TileViewModel> Items = new List<TileViewModel>();

        public TileProductViewModelFactory(IRepository repository, User user, Guid categoryId, int count, int offset)
            : base(repository, user, categoryId, count, offset)
        {
        }

        protected override object CreateViewModel(PaginationViewModel pagination)
        {
            return new TilesViewModel { Pagination = pagination, Tiles = this.Items };
        }

        protected override void AddItem(Product product, CurrencyString price)
        {
            var tileViewModel = new TileViewModel
            {
                Link = new NavigationLink { PageId = product.VersionId },
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

            this.Items.Add(tileViewModel);        
        }
    }

    public class ListProductViewModelFactory : ProductViewModelFactoryBase, IProductViewModelFactory
    {
        private IList<TableRowViewModel> Items = new List<TableRowViewModel>();

        public ListProductViewModelFactory(IRepository repository, User user, Guid categoryId, int count, int offset)
            : base(repository, user, categoryId, count, offset)
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

            return new TableViewModel 
            { 
                Pagination = pagination,
                Header = headers,
                Rows = this.Items,
                RowClickAction = new NavigationLink("Page")
            };
        }

        protected override void AddItem(Product product, CurrencyString price)
        {
            var row = user != null
                ? new List<TableCellViewModel>
                {
                    new TableCellViewModel(new ImageThumbnailViewModel{
                        ThumbnailId = product.MainImageId ?? Guid.Empty,
                        Width = 64
                    }),
                    new TableCellViewModel(product.Name),
                    new TableCellViewModel(this.GetProductAvailableString(product)),
                    new TableCellViewModel(price != null ? price.ToString() : StringResource.NotAvailable),
                    new TableCellViewModel(this.CreateAddToCartLink(product))
                }
                : new List<TableCellViewModel>
                {
                    new TableCellViewModel(new ImageThumbnailViewModel{
                        ThumbnailId = product.MainImageId ?? Guid.Empty,
                        Width = 64
                    }),
                    new TableCellViewModel(product.Name)
                };

            this.Items.Add(new TableRowViewModel
            {
                Cells = row,
                Id = product.Id.ToString()
            });        
        }
    }
}