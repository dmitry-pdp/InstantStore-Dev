using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                return context.OrderProducts.Where(x => x.OrderId == order.Id).Sum(x => x.Count);
            }
        }

        public Order AddItemToCurrentOrder(User user, Guid productId, int count)
        {
            using(var context = new InstantStoreDataContext())
            {
                var order = context.Orders.FirstOrDefault(x => x.UserId == user.Id && x.Status == (int)OrderStatus.Active);
                if (order == null)
                {
                    order = this.CreateOrderForUser(context, user);
                }

                context.OrderProducts.InsertOnSubmit(new OrderProduct
                {
                    Id = Guid.NewGuid(),
                    Count = count,
                    OrderId = order.Id,
                    ProductId = productId                
                });

                context.SubmitChanges();

                return order;
            }
        }

        private Order CreateOrderForUser(InstantStoreDataContext context, User user)
        {
            var order = new Order {
                Id = Guid.NewGuid(),
                UserId = user.Id,
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
    }
}
