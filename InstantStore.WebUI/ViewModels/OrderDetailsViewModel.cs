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
using System.Globalization;

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
                        Thumbnail = new ImageThumbnailViewModel { ThumbnailId = product.MainImageId ?? Guid.Empty, Width = 100 },
                        Title = new NavigationLink { Text = product.Name, ActionName = "Page", PageId = product.VersionId },
                        Count = itemsCount,
                        IsAvailable = product.IsAvailable,
                        ItemPrice = product.IsAvailable ? new CurrencyString(itemPrice, currency.Text) : null,
                        TotalPrice = product.IsAvailable ? new CurrencyString(itemsPrice, currency.Text) : null
                    };

                    this.Products.Add(viewModel);
                }

                order.TotalPrice = total;

                this.Offer = this.GetOffer(order, currency);

                if (this.Offer != null)
                {
                    total = this.Offer.Type == OfferDiscountType.Percent
                        ? total * (1 - this.Offer.Discount / 100.0M)
                        : total - this.Offer.Discount;

                    using (var context = new InstantStoreDataContext())
                    {
                        var o = context.Orders.FirstOrDefault(x => x.Id == order.Id);
                        if (o != null)
                        {
                            o.OfferId = this.Offer.Id;
                            o.TotalPrice = total;
                            context.SubmitChanges();
                        }
                    }
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
                        Thumbnail = new ImageThumbnailViewModel { ThumbnailId = product.MainImageId ?? Guid.Empty, Width = 100 },
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
            var processedUpdate = orderStatuses.FirstOrDefault(x => x.Status == (int)OrderStatus.Completed);

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

        public string UserName { get; set; }

        public OrderStatus Status { get; set; }

        public OfferViewModel Offer { get; set; }

        public string GetOfferText()
        {
            if (this.Offer == null)
            {
                return null;
            }

            return this.Offer.Type == OfferDiscountType.Percent
                ? string.Format(StringResource.PercentDiscountFormat, this.Offer.Discount.ToString("N", new CultureInfo("ru-RU")))
                : new InstantStore.WebUI.ViewModels.Helpers.CurrencyString(this.Offer.Discount, this.Offer.CurrencyText).ToString();
        }

        private OfferViewModel GetOffer(Order order, Currency currency)
        {
            using (var context = new InstantStoreDataContext())
            {
                var availableOffer = context.Offers
                    .Where(
                        offer =>
                        offer.IsActive &&
                        offer.CurrencyId == currency.Id &&
                        offer.ThresholdPriceValue <= order.TotalPrice)
                    .OrderByDescending(
                        offer =>
                        offer.ThresholdPriceValue)
                    .FirstOrDefault();

                if (availableOffer != null)
                {
                    return new OfferViewModel
                    {
                        Id = availableOffer.VersionId,
                        Name = availableOffer.Name,
                        Type = (OfferDiscountType)availableOffer.DiscountType,
                        Discount = availableOffer.DiscountValue,
                        CurrencyText = availableOffer.Currency.Text
                    };
                }
            }

            return null;
        }
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