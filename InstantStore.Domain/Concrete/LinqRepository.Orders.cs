using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InstantStore.Domain.Exceptions;
using InstantStore.Domain.Helpers;

namespace InstantStore.Domain.Concrete
{
    public enum OrderStatus : int
    {
        Unknown = 0,
        Active = 1,
        Placed = 2,
        Processed = 3
    }

    public partial class LinqRepository
    {
        public int GetOrderItemsCount(User user)
        {
            using (var context = new InstantStoreDataContext())
            {
                var order = context.Orders.FirstOrDefault(x => x.UserId == user.Id && x.Status == (int)OrderStatus.Active);
                if (order == null)
                {
                    return -1;
                }

                var productOrders = context.OrderProducts.Where(x => x.OrderId == order.Id).ToList();
                if (!productOrders.Any())
                {
                    return 0;
                }

                return productOrders.Sum(x => x.Count);
            }
        }

        public Order AddItemToCurrentOrder(User user, Guid productId, int count)
        {
            using(var context = new InstantStoreDataContext())
            {
                var order = context.Orders.FirstOrDefault(x => x.UserId == user.Id && x.Status == (int)OrderStatus.Active);
                if (order == null)
                {
                    order = this.CreateOrderForUser(context, user.Id);
                }

                if (user.DefaultCurrencyId == null || user.DefaultCurrencyId == Guid.Empty)
                {
                    throw new ModelValidationException("User has no currency assigned.");
                }

                if (order.Status != (int)OrderStatus.Active)
                {
                    throw new ModelValidationException("Order has already been submitted.");
                }

                AddOrUpdateOrderItem(user, productId, count, context, order);

                context.SubmitChanges();

                return order;
            }
        }

        private void AddOrUpdateOrderItem(User user, Guid productId, int count, InstantStoreDataContext context, Order order)
        {
            var product = context.Products.FirstOrDefault(x => x.VersionId == productId);
            if (product == null)
            {
                throw new ModelValidationException("There are no product for the id.");
            }

            var existingOrder = context.OrderProducts.FirstOrDefault(x => x.ProductId == productId && x.OrderId == order.Id);
            if (existingOrder == null)
            {
                context.OrderProducts.InsertOnSubmit(new OrderProduct
                {
                    Id = Guid.NewGuid(),
                    Count = count,
                    OrderId = order.Id,
                    ProductId = productId,
                    Price = product.GetPriceForUser(user, context.ExchangeRates),
                    PriceCurrencyId = user.DefaultCurrencyId.Value
                });
            }
            else
            {
                existingOrder.Count += count;
                existingOrder.Price = product.GetPriceForUser(user, context.ExchangeRates);
                existingOrder.PriceCurrencyId = user.DefaultCurrencyId.Value;
            }
        }

        private Order CreateOrderForUser(InstantStoreDataContext context, Guid userId)
        {
            var order = new Order {
                Id = Guid.NewGuid(),
                UserId = userId,
                Status = (int)OrderStatus.Active
            };

            context.Orders.InsertOnSubmit(order);
            context.OrderUpdates.InsertOnSubmit(new OrderUpdate
            {
                Id = Guid.NewGuid(),
                DateTime = DateTime.Now,
                OrderId = order.Id,
                Status = (int)OrderStatus.Active
            });

            return order;
        }

        public Order GetActiveOrder(Guid userId)
        {
            using(var context = new InstantStoreDataContext())
            {
                var order = context.Orders.FirstOrDefault(x => x.UserId == userId && x.Status == (int)OrderStatus.Active);
                return order == null ? this.CreateOrderForUser(context, userId) : order;
            }
        }

        public Order GetOrderById(Guid orderId)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.Orders.FirstOrDefault(x => x.Id == orderId);
            }
        }

        public IList<KeyValuePair<OrderProduct, Product>> GetProductsForOrder(Guid orderId)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.OrderProducts
                    .Where(x => x.OrderId == orderId)
                    .Join(context.Products, x => x.ProductId, y => y.VersionId, (x, y) => new KeyValuePair<OrderProduct, Product>(x, y))
                    .ToList();
            }
        }

        public void DeleteOrderProduct(Guid orderProductId)
        {
            using (var context = new InstantStoreDataContext())
            {
                var orderProduct = context.OrderProducts.FirstOrDefault(x => x.Id == orderProductId);
                if (orderProduct != null)
                {
                    context.OrderProducts.DeleteOnSubmit(orderProduct);
                    context.SubmitChanges();
                }
            }
        }

        public void UpdateOrderProduct(OrderProduct orderProduct)
        {
            using (var context = new InstantStoreDataContext())
            {
                var orderProductUpdated = context.OrderProducts.FirstOrDefault(x => x.Id == orderProduct.Id);
                if (orderProductUpdated != null)
                {
                    orderProductUpdated.Count = orderProduct.Count;
                    orderProductUpdated.Price = orderProduct.Price;
                    orderProductUpdated.PriceCurrencyId = orderProduct.PriceCurrencyId;
                    context.SubmitChanges();
                }
            }
        }

        public void SubmitOrder(Guid orderId)
        {
            using (var context = new InstantStoreDataContext())
            {
                var order = context.Orders.FirstOrDefault(x => x.Id == orderId);
                if (order != null)
                {
                    if (!order.OrderProducts.Any())
                    {
                        throw new ModelValidationException("NoProducts");
                    }

                    if (order.User.DefaultCurrencyId == null)
                    {
                        throw new ModelValidationException("NoUserCurrencySet");
                    }

                    Guid userCurrency = order.User.DefaultCurrencyId.Value;

                    if (!order.OrderProducts.All(x => x.PriceCurrencyId == userCurrency))
                    {
                        throw new ModelValidationException("ProductCurrencyIsInvalid");
                    }

                    if (order.OrderProducts.Any(x => x.Price <= 0))
                    {
                        throw new ModelValidationException("ProductPriceIsNotSet");
                    }

                    order.Status = (int)OrderStatus.Placed;
                    order.TotalPrice = order.OrderProducts.Sum(x => x.Price * x.Count);
                    order.PriceCurrencyId = userCurrency;
                    
                    context.OrderUpdates.InsertOnSubmit(new OrderUpdate
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderId,
                        Status = order.Status,
                        DateTime = DateTime.Now
                    });

                    context.SubmitChanges();
                }
            }
        }

        public IList<Order> GetOrdersWithStatus(IEnumerable<OrderStatus> statuses, Guid? userId, int offset, int count)
        {
            using (var context = new InstantStoreDataContext())
            {
                var orders = context.Orders
                    .Where(x => statuses.Contains((OrderStatus)x.Status) && (userId == null || x.UserId == userId))
                    .Skip(offset);

                if (count > 0)
                { 
                    orders = orders.Take(count);
                }
                
                return orders.ToList();
            }
        }

        public IList<OrderUpdate> GetStatusesForOrder(Guid orderId)
        {
            using (var context = new InstantStoreDataContext())
            {
                return context.OrderUpdates.Where(x => x.OrderId == orderId).ToList();
            }
        }

        public void RemoveOrderProduct(Guid orderProductId)
        {
            using (var context = new InstantStoreDataContext())
            {
                var orderProduct = context.OrderProducts.FirstOrDefault(x => x.Id == orderProductId);
                if (orderProduct != null)
                {
                    context.OrderProducts.DeleteOnSubmit(orderProduct);
                    context.SubmitChanges();
                }
            }
        }

        public void AddProductsFromOrder(Guid orderId, User user)
        {
            using (var context = new InstantStoreDataContext())
            {
                var order = context.Orders.FirstOrDefault(x => x.UserId == user.Id && x.Status == (int)OrderStatus.Active);
                order = order == null ? this.CreateOrderForUser(context, user.Id) : order;

                var originalOrder = context.Orders.FirstOrDefault(x => x.Id == orderId);
                if (originalOrder == null)
                {
                    return;
                }

                foreach(var orderProduct in originalOrder.OrderProducts)
                {
                    this.AddOrUpdateOrderItem(user, orderProduct.ProductId, orderProduct.Count, context, order);
                }

                context.SubmitChanges();
            }
        }
    }
}
