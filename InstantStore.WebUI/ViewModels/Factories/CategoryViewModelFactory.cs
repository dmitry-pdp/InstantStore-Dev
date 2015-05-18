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

        public static TilesViewModel GetProductsForCategory(this IRepository repository, User user, Guid categoryId, int count, int offset = 0)
        {
            var products = repository.GetProductsForCategory(categoryId, offset, count);
            if (products == null)
            {
                return null;
            }

            var maxProducts = repository.GetProductsCountForCategory(categoryId);
            var maxPages = maxProducts / count;
            var pagination = new PaginationViewModel { 
                MaxPages = maxPages,
                CurrentPage = maxProducts != 0 ? offset * maxPages / maxProducts : 0,
                Count = count
            };

            var tilesViewModel = new TilesViewModel() { Tiles = new List<TileViewModel>(), Pagination = pagination };

            var currency = user != null && user.DefaultCurrencyId != null && !user.IsAdmin
                ? repository.GetCurrencies().FirstOrDefault(x => x.Id == user.DefaultCurrencyId) 
                : null; 

            var exchangeRates = user != null && user.DefaultCurrencyId != null 
                ? repository.GetExchangeRates().Where(x => x.ToCurrencyId == user.DefaultCurrencyId)
                : new List<ExchangeRate>();

            foreach(var product in products)
            {
                var tileViewModel = new TileViewModel
                {
                    Link = new NavigationLink { PageId = product.VersionId },
                    ImageId = product.MainImageId ?? Guid.Empty,
                    Name = product.Name
                };

                if (user != null)
                {
                    var attributes = new AttributeList();
                    attributes.Add(new KeyValuePair<string, string>(StringResource.productTile_Available, product.IsAvailable ? StringResource.Yes : StringResource.No));

                    if (currency != null && !user.IsAdmin)
                    {
                        var price = product.GetPriceForUser(user, repository.GetExchangeRates());
                        attributes.Add(new KeyValuePair<string, string>(
                            StringResource.productTile_Price, 
                            new CurrencyString(price, currency.Text).ToString()));
                    }
                    
                    tileViewModel.Attributes = attributes;

                    if (!user.IsAdmin)
                    {
                        tileViewModel.Action = new NavigationLink { ControllerName = "Main", ActionName = "AddToCart", PageId = product.VersionId, Text = StringResource.productTile_AddToCart };
                    }
                }

                tilesViewModel.Tiles.Add(tileViewModel);
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
}