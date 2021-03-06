﻿using InstantStore.Domain.Abstract;
using InstantStore.WebUI.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace InstantStore.WebUI.Controllers
{
    public class ProductController : Controller
    {
        IRepository repository;
        public int PageSize = 25; //Default value is overriden in constructor.

        public ProductController(IRepository productRepository)
        {
            this.repository = productRepository;
            Int32.TryParse(System.Configuration.ConfigurationManager.AppSettings["ProductPageSize"], out this.PageSize);
        }

        public ViewResult List(string category, int page = 1)
        {
            /*
            var products = repository.Products;

            //if No Products, Uuupsie!
            //System.Diagnostics.Trace.Assert((), Resources.StringResource.ProductList_NoProducts_Error,
            //String.Format("ProductController, List, page={0}", page));

            ProductListViewModel model = new ProductListViewModel
            {
                Products = products != null ?
                products.Where(p => String.IsNullOrEmpty(category) || p.Category.URLAlias == category)
                .OrderBy(p => p.SortOrder)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                : null,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = PageSize,
                    TotalItems = products != null ? repository.Products.Count() : 0
                },
                CurrentCategory = category
            };
            return View(model);
            */

            return null;
        }

    }
}
