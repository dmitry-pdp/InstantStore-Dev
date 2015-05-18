using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.Domain.Helpers;
using InstantStore.WebUI.Resources;
using InstantStore.WebUI.ViewModels.Helpers;

namespace InstantStore.WebUI.ViewModels
{
    public class OrderDetailsViewModel
    {
        public OrderDetailsViewModel()
        {
        }

        public OrderDetailsViewModel(IRepository repository, User user)
            : this(repository, repository.GetActiveOrder(user.Id), user)
        {
        }

        public OrderDetailsViewModel(IRepository repository, Order order, User user)
        {
            if (order == null)
            {
                throw new ApplicationException("InconsistentModel");
            }

            this.Id = order.Id;

            var currencies = repository.GetCurrencies();
            var exchangeRates = repository.GetExchangeRates();
            var products = repository.GetProductsForOrder(order.Id);

            var currency = currencies.FirstOrDefault(x => x.Id == user.DefaultCurrencyId);
            if (currency == null)
            {
                throw new ApplicationException("InconsistentModel");
            }

            if (products != null || products.Any())
            {
                this.Products = new List<ProductOrderViewModel>();

                decimal total = 0.0M;

                foreach (var orderProductGroup in products.GroupBy(x => x.Key.ProductId))
                {
                    var orderProductGroupItem = orderProductGroup.First();

                    var product = orderProductGroupItem.Value;
                    if (product == null)
                    {
                        throw new ApplicationException("InconsistentModel");
                    }

                    var orderProduct = orderProductGroupItem.Key;

                    var itemsCount = orderProductGroup.Sum(x => x.Key.Count);
                    var itemPrice = product.GetPriceForUser(user, exchangeRates);
                    var itemsPrice = itemsCount * itemPrice;

                    if (product.IsAvailable)
                    {
                        total += itemsPrice;
                    }

                    var viewModel = new ProductOrderViewModel
                    {
                        Id = orderProduct.Id,
                        Thumbnail = new ImageThumbnailViewModel { ThumbnailId = product.MainImageId, Width = 100 },
                        Title = new NavigationLink { Text = product.Name, ActionName = "Page", PageId = product.VersionId },
                        Count = itemsCount,
                        IsAvailable = product.IsAvailable,
                        ItemPrice = product.IsAvailable ? new CurrencyString(itemPrice, currency.Text) : null,
                        TotalPrice = product.IsAvailable ? new CurrencyString(itemsPrice, currency.Text) : null
                    };

                    this.Products.Add(viewModel);
                }

                this.Total = new CurrencyString(total, currency.Text);
            }
        }

        public OrderDetailsViewModel Load(IRepository repository, Order order, User user)
        {
            this.Id = order.Id;
            var currency = repository.GetCurrencies().FirstOrDefault(x => x.Id == order.PriceCurrencyId);
            if (currency == null)
            {
                throw new ApplicationException("InconsistentModel");
            }

            this.Total = order.TotalPrice != null ? new CurrencyString(order.TotalPrice.Value, currency.Text) : null;
            this.Products = new List<ProductOrderViewModel>();

            var products = repository.GetProductsForOrder(order.Id);
            if (products != null || products.Any())
            {
                foreach (var orderProductPair in products)
                {
                    var orderProduct = orderProductPair.Key;
                    var product = orderProductPair.Value;
                    this.Products.Add(new ProductOrderViewModel
                    {
                        Id = orderProduct.Id,
                        Thumbnail = new ImageThumbnailViewModel { ThumbnailId = product.MainImageId, Width = 100 },
                        Title = new NavigationLink { Text = product.Name, ActionName = "Page", PageId = product.VersionId },
                        Count = orderProduct.Count,
                        IsAvailable = product.IsAvailable,
                        ItemPrice = new CurrencyString(orderProduct.Price, currency.Text)
                    });
                }
            }

            this.Description = order.Comment;
            this.SetOrderDates(repository, order);
            return this;
        }

        public void SetOrderDates(IRepository repository, Order order)
        {
            var orderStatuses = repository.GetStatusesForOrder(order.Id);
            var submittedUpdate = orderStatuses.FirstOrDefault(x => x.Status == (int)OrderStatus.Placed);
            var processedUpdate = orderStatuses.FirstOrDefault(x => x.Status == (int)OrderStatus.Processed);

            this.SubmitDate = submittedUpdate != null ? (DateTime?)submittedUpdate.DateTime : null;
            this.ProcessedDate = processedUpdate != null ? (DateTime?)processedUpdate.DateTime : null;
        }

        public int ItemsCount { get; set; }

        public DateTime? SubmitDate { get; set; }

        public DateTime? ProcessedDate { get; set; }

        public List<ProductOrderViewModel> Products { get; set; }

        public CurrencyString Total { get; set; }

        public Guid Id { get; set; }

        public string Description { get; set; }
    }

    public class ProductOrderViewModel
    {
        public Guid Id { get; set; }

        public NavigationLink Title { get; set; }

        public bool IsAvailable { get; set; }

        public ImageThumbnailViewModel Thumbnail { get; set; }

        public int Count { get; set; }

        public CurrencyString ItemPrice { get; set; }

        public CurrencyString TotalPrice { get; set; }
    }
}