using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.Domain.Helpers;
using InstantStore.WebUI.Models;
using InstantStore.WebUI.Resources;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.ViewModels.Factories;
using System.Globalization;

namespace InstantStore.WebUI.Controllers
{
    public partial class UserController
    {
        public ActionResult History(int offset = 0, int count = 25)
        {
            this.Initialize(Guid.Empty, PageIdentity.History);
            return this.View(new OrderHistoryListViewModel(this.repository, this.currentUser, offset, count));
        }

        public ActionResult HistoryOrderDetails(Guid? id)
        {
            this.Initialize(Guid.Empty);

            Order order = id != null && id.Value != Guid.Empty ? this.repository.GetOrderById(id.Value) : null;
            if (order == null)
            {
                return this.HttpNotFound();
            }

            return this.View(new OrderDetailsViewModel().Load(this.repository, order, this.currentUser));
        }

        public ActionResult Orders(Guid? id, string a)
        {
            this.Initialize(Guid.Empty, PageIdentity.Cart);
            var user = UserIdentityManager.GetActiveUser(this.Request, this.repository);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            if (a == "delete" && id != null)
            {
                repository.DeleteOrderProduct(id.Value);
            }

            return this.View("Orders", new OrderDetailsViewModel(this.repository, user));
        }

        [HttpPost]
        public ActionResult Recalculate(OrderDetailsViewModel viewModel)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, this.repository);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            var order = this.repository.GetOrderById(viewModel.Id);
            if (order == null || order.Status != (int)OrderStatus.Active)
            {
                return this.HttpNotFound();
            }

            var orderProducts = this.repository.GetProductsForOrder(order.Id);
            if (orderProducts.Count != viewModel.Products.Count || !orderProducts.All(x => viewModel.Products.Any(y => y.Id == x.Key.Id)))
            {
                throw new ApplicationException("Cart contents is inconsistent.");
            }

            // update the cart items count.
            foreach (var orderProduct in viewModel.Products)
            {
                var orderProductPair = orderProducts.Where(x => x.Key.Id == orderProduct.Id).First();
                var product = orderProductPair.Value;
                var orderItem = orderProductPair.Key;

                if (!product.IsAvailable)
                {
                    continue;
                }

                orderItem.Count = orderProduct.Count;
                orderItem.Price = product.GetPriceForUser(user, this.repository.GetExchangeRates());
                orderItem.PriceCurrencyId = user.DefaultCurrencyId.Value;
                this.repository.UpdateOrderProduct(orderItem);
            }

            return this.Orders(null, null);
        }

        [HttpPost]
        public ActionResult PlaceOrder(OrderDetailsViewModel viewModel)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, this.repository);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            var order = this.repository.GetOrderById(viewModel.Id);
            if (order == null || order.Status != (int)OrderStatus.Active)
            {
                return this.HttpNotFound();
            }

            var orderProducts = this.repository.GetProductsForOrder(order.Id);
            if (orderProducts.Count != viewModel.Products.Count || !orderProducts.All(x => viewModel.Products.Any(y => y.Id == x.Key.Id)))
            {
                throw new ApplicationException("Cart contents is inconsistent.");
            }

            var orderProductsToRemove = new List<Guid>();
            foreach (var orderProduct in orderProducts.Where(x => !viewModel.Products.Any(y => y.Id == x.Key.Id)))
            {
                this.repository.RemoveOrderProduct(orderProduct.Key.Id);
            }

            this.repository.SubmitOrder(order.Id);

            var orderSubmitDate = this.repository.GetStatusesForOrder(order.Id).FirstOrDefault(x => x.Status == (int)OrderStatus.Placed);

            EmailManager.Send(
                user,
                this.repository,
                EmailType.EmailOrderHasBeenPlaced,
                new Dictionary<string, string> { 
                        { "%order.id%", order.Id.ToString() }, 
                        { "%order.user%", order.User.Name }, 
                        { "%order.date%", orderSubmitDate != null ? orderSubmitDate.DateTime.ToString("F", new CultureInfo("ru-RU")) : string.Empty } 
                    });

            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddToCart(Guid id, int count = 1)
        {
            if (id == Guid.Empty)
            {
                return this.HttpNotFound();
            }

            var user = UserIdentityManager.GetActiveUser(this.Request, this.repository);
            if (user == null || !user.IsActivated)
            {
                return this.HttpNotFound();
            }

            this.repository.AddItemToCurrentOrder(user, id, count);

            return this.Json(new { result = "success" });
        }

        public ActionResult CopyOrder(Guid id)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, this.repository);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            this.repository.AddProductsFromOrder(id, user);

            return this.RedirectToAction("Orders");
        }
    }
}