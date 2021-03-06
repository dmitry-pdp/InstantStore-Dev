﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Resources;
using System.ComponentModel.DataAnnotations;
using InstantStore.WebUI.ViewModels.Factories;

namespace InstantStore.WebUI.ViewModels
{
    public class CategoryProductsViewModel
    {
        private IRepository repository;

        public CategoryProductsViewModel()
        {

        }

        public CategoryProductsViewModel(IRepository repository, Guid id, int offset, int count)
        {
            this.repository = repository;
            this.ParentCategoryId = id;
            this.Pagination = new PaginationViewModel() {
                MaxPages = (repository.GetProductsCountForCategory(id) / count) + 1,
                CurrentPage = (offset / count) + 1,
                Count = count,
                Id = "0"
            };
            this.Products = repository.GetProductsForCategory(id, offset, count).Select(x => CreateItemViewModel(x.Item3)).ToList();
        }

        public List<CategoryProductViewModel> Products { get; set; }

        public Guid ParentCategoryId { get; set; }

        public bool IsTiles { get; set; }

        public PaginationViewModel Pagination { get; set; }
    
        public CategoryProductViewModel CreateItemViewModel(Product product)
        {
            return new CategoryProductViewModel
            {
                Id = product.VersionId,
                Name = product.Name,
                Image = repository.GetImagesForProduct(product.Id).FirstOrDefault(),
                Currency = repository.GetCurrencies().First(x => x.Id == product.PriceCurrencyId).Text,
                PriceCash = (product.PriceValueCash != null ? (float)product.PriceValueCash : 0.0f).ToString(),
                PriceCashless = (product.PriceValueCashless != null ? (float)product.PriceValueCashless : 0.0f).ToString(),
                IsAvailable = product.IsAvailable ? StringResource.Yes : StringResource.No
            };
        }
    }

    public class CategoryProductViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        
        public string IsAvailable { get; set; }

        public string Currency { get; set; }

        public string PriceCash { get; set; }

        public string PriceCashless { get; set; }

        public Guid Image { get; set; }

        public bool Checked { get; set; }
    }
}