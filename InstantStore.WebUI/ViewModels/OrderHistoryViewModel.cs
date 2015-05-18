using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.Domain.Entities;
using InstantStore.WebUI.Resources;
using InstantStore.WebUI.ViewModels.Helpers;

namespace InstantStore.WebUI.ViewModels
{
    public class OrderHistoryListViewModel
    {
        public OrderHistoryListViewModel(IRepository repository, User user, int offset, int count)
        {
            this.Orders = repository
                .GetOrdersWithStatus(new[] { OrderStatus.Placed, OrderStatus.Processed }, user.IsAdmin ? (Guid?)null : user.Id, offset, count)
                .Select(order => CreateOrderDetail(repository, order))
                .ToList();
        }

        public List<OrderDetailsViewModel> Orders { get; set; }

        private OrderDetailsViewModel CreateOrderDetail(IRepository repository, Order order)
        {
            var orderStatuses = repository.GetStatusesForOrder(order.Id);
            var currency = repository.GetCurrencies().FirstOrDefault(x => order.PriceCurrencyId == x.Id);
            var submittedUpdate = orderStatuses.FirstOrDefault(x => x.Status == (int)OrderStatus.Placed);
            var processedUpdate = orderStatuses.FirstOrDefault(x => x.Status == (int)OrderStatus.Processed);

            return new OrderDetailsViewModel
            {
                Id = order.Id,
                ItemsCount = repository.GetProductsForOrder(order.Id).Sum(x => x.Key.Count),
                Total = order.TotalPrice != null ? new CurrencyString(order.TotalPrice.Value, currency.Text) : null,
                SubmitDate = submittedUpdate != null ? (DateTime?)submittedUpdate.DateTime : null,
                ProcessedDate = processedUpdate != null ? (DateTime?)processedUpdate.DateTime : null
            };
        }
    }
}