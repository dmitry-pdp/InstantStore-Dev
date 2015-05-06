using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Resources;

namespace InstantStore.WebUI.ViewModels.Factories
{
    public static class CategoryViewModelFactory
    {
        public static TilesViewModel GetTopCategories(this IRepository repository)
        {
            var topCategories = repository.GetPages(null, page => page.CategoryId != null).ToList();
            if (topCategories.Count == 0)
            {
                return null;
            }

            var tilesViewModel = new TilesViewModel() { Tiles = new List<TileViewModel>() };

            foreach(var categoryPage in topCategories)
            {
                var categoryDetais = repository.GetCategoryById(categoryPage.CategoryId.Value);
                tilesViewModel.Tiles.Add(new TileViewModel {
                    Id = categoryPage.Id,
                    Name = categoryPage.Name,
                    ImageId = categoryDetais.ImageId ?? Guid.Empty
                });
            }

            return tilesViewModel;
        }

        public static TilesViewModel GetProductsForCategory(this IRepository repository, Guid categoryId, int count, int offset = 0)
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

            foreach(var product in products)
            {
                tilesViewModel.Tiles.Add(new TileViewModel { 
                    Id = product.Id,
                    ImageId = product.MainImageId ?? Guid.Empty,
                    Name = product.Name
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
                Link = new NavigationLink { PageId = product.Id },
                ImageThumbnailId = product.MainImageId
            })
            .ToList();

            return viewModel;
        }
    }
}