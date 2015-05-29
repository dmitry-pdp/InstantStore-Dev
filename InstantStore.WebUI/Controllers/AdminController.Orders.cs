using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Models;
using InstantStore.WebUI.Resources;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.ViewModels.Factories;
using InstantStore.WebUI.ViewModels.Helpers;

namespace InstantStore.WebUI.Controllers
{
    public partial class AdminController
    {
        private static Dictionary<char, OrderStatus> statusMap = new Dictionary<char, OrderStatus>
        {
            {'p', OrderStatus.Placed },
            {'i', OrderStatus.InProcess },
            {'c', OrderStatus.Completed }
        };

        private static Dictionary<OrderStatus, string> orderStatusMap = new Dictionary<OrderStatus, string>
        {
            { OrderStatus.Placed, StringResource.orderStatus_Placed },
            { OrderStatus.InProcess, StringResource.orderStatus_InProcess },
            { OrderStatus.Completed, StringResource.orderStatus_Processed }
        };

        public ActionResult Orders(char t = 'p', int o = 0, int c = 50)
        {
            var user = this.HttpContext.CurrentUser();

            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Orders);
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            
            OrderStatus status;
            if (!statusMap.TryGetValue(t, out status))
            {
                status = OrderStatus.Placed;
            }

            using (var context = new InstantStoreDataContext())
            {
                Func<Order, bool> selector = (Order order) => (int)status == order.Status;
                Func<OrderUpdate, bool> orderSortDateSelector = (OrderUpdate update) => update.Status == (int)status;
                this.ViewData["OrdersTableViewModel"] = new TableViewModel
                {
                    Header = new List<TableCellViewModel>
                    {
                        new TableCellViewModel(StringResource.user_HistoryListHeaderTime),
                        new TableCellViewModel(StringResource.user_HistoryListHeaderUserName),
                        new TableCellViewModel(StringResource.user_HistoryListHeaderItemCount),
                        new TableCellViewModel(StringResource.user_HistoryListHeaderTotalPrice),
                        new TableCellViewModel(StringResource.user_HistoryListHeaderProcessTime)
                    },
                    Rows = context.Orders
                        .Where(selector)
                        .OrderByDescending(order => order.OrderUpdates
                            .Where(orderSortDateSelector)
                            .OrderByDescending(u => u.DateTime)
                            .First().DateTime)
                        .Skip(o)
                        .Take(c)
                        .Select(ConvertOrderToTableRow)
                        .ToList(),
                    RowClickAction = new NavigationLink("Order"),
                    Pagination = new PaginationViewModel(c, o, context.Orders.Count(selector)) 
                    {
                        Link = new NavigationLink("Orders", "Admin") 
                        { 
                            Parameters = new { t = t } 
                        }
                    }
                };

                this.ViewData["OrdersHeaderViewModel"] = new TabControlViewModel
                {
                    Tabs = statusMap.Select(key => CreateOrderStatusHeader(key, t)).ToList()
                };
            }

            return this.View();
        }

        private TableRowViewModel ConvertOrderToTableRow(Order order)
        {
            var submittedUpdate = order.OrderUpdates.FirstOrDefault(update => update.Status == (int)OrderStatus.Placed);
            var completeUpdate = order.OrderUpdates.FirstOrDefault(update => update.Status == (int)OrderStatus.Completed);

            return new TableRowViewModel
            {
                Id = order.Id.ToString(),
                Cells = new List<TableCellViewModel>
                {
                    new TableCellViewModel(submittedUpdate != null ? submittedUpdate.DateTime.ToString(russianCulture) : "<N/A>"),
                    new TableCellViewModel(string.Concat(order.User.Name, ", ", order.User.City)),
                    new TableCellViewModel(order.OrderProducts.Sum(orderItem => orderItem.Count).ToString(russianCulture)),
                    new TableCellViewModel(order.TotalPrice != null ? new CurrencyString(order.TotalPrice.Value, order.Currency.Text).ToString() : "<N/A>"),
                    new TableCellViewModel(completeUpdate != null ? completeUpdate.DateTime.ToString(russianCulture) : StringResource.NotAvailable)
                }
            };
        }

        private BreadcrumbItemViewModel CreateOrderStatusHeader(KeyValuePair<char, OrderStatus> data, char current)
        {
            return new BreadcrumbItemViewModel
            {
                IsActive = data.Key == current,
                Name = orderStatusMap[data.Value],
                Link = new NavigationLink("Orders", "Admin") { Parameters = new { t = data.Key } }
            };
        }

        public ActionResult Order(Guid id)
        {
            var user = this.HttpContext.CurrentUser();

            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Orders);
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            
            using (var context = new InstantStoreDataContext())
            {
                var order = context.Orders.FirstOrDefault(x => x.Id == id);
                if (order == null)
                {
                    return this.HttpNotFound();
                }

                this.ViewData["OrderStatusList"] = new[] { OrderStatus.Placed, OrderStatus.InProcess, OrderStatus.Completed}
                    .Select(status => new SelectListItem
                    {
                        Text = orderStatusMap[status],
                        Selected = (OrderStatus)order.Status == status,
                        Value = status.ToString()
                    }).ToList();

                var submittedUpdate = order.OrderUpdates.FirstOrDefault(update => update.Status == (int)OrderStatus.Placed);
                var completeUpdate = order.OrderUpdates.FirstOrDefault(update => update.Status == (int)OrderStatus.Completed);
                
                return this.View(new OrderDetailsViewModel
                {
                    Id = order.Id,
                    Status = (OrderStatus)order.Status,
                    Products = order.OrderProducts.Select(CreateProductOrderViewModel).ToList(),
                    ItemsCount = order.OrderProducts.Sum(orderItem => orderItem.Count),
                    Total = order.TotalPrice != null ? new CurrencyString(order.TotalPrice.Value, order.Currency.Text) : null,
                    Description = order.Comment,
                    UserName = string.Concat(order.User.Name, ", ", order.User.City),
                    SubmitDate = submittedUpdate != null ? (DateTime?)submittedUpdate.DateTime : null,
                    ProcessedDate = completeUpdate != null ? (DateTime?)completeUpdate.DateTime : null,
                    Offer = order.Offer != null ? new OfferViewModel
                    {
                        Name = order.Offer.Name,
                        Type = (OfferDiscountType)order.Offer.DiscountType,
                        Discount = order.Offer.DiscountValue,
                        CurrencyText = order.Offer.Currency.Text
                    } : null
                });
            }
        }

        private ProductOrderViewModel CreateProductOrderViewModel(OrderProduct orderProduct)
        {
            return new ProductOrderViewModel
            {
                Id = orderProduct.Id,
                Thumbnail = new ImageThumbnailViewModel 
                { 
                    ThumbnailId = orderProduct.Product.MainImageId ?? Guid.Empty, 
                    Width = 100 
                },
                Title = new NavigationLink 
                { 
                    Text = orderProduct.Product.Name, 
                    ActionName = "Product", 
                    ControllerName = "Admin", 
                    PageId = orderProduct.Product.VersionId 
                },
                Count = orderProduct.Count,
                IsAvailable = orderProduct.Product.IsAvailable,
                ItemPrice = new CurrencyString(orderProduct.Price, orderProduct.Currency.Text)
            };
        }

        [HttpPost]
        public ActionResult Order(OrderDetailsViewModel orderViewModel)
        {
            var user = UserIdentityManager.GetActiveUser(this.Request, repository);
            if (user == null || !user.IsAdmin || orderViewModel == null)
            {
                return this.HttpNotFound();
            }

            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Orders);
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;

            using (var context = new InstantStoreDataContext())
            {
                var order = context.Orders.FirstOrDefault(x => x.Id == orderViewModel.Id);
                if (order == null)
                {
                    return this.HttpNotFound();
                }

                if (order.Comment != orderViewModel.Description ||
                    order.Status != (int)orderViewModel.Status)
                {
                    order.Comment = orderViewModel.Description;
                    if (order.Status != (int)orderViewModel.Status)
                    {
                        order.Status = (int)orderViewModel.Status;
                        context.OrderUpdates.InsertOnSubmit(new OrderUpdate
                        {
                            Status = (int)orderViewModel.Status,
                            DateTime = DateTime.Now,
                            Id = Guid.NewGuid(),
                            OrderId = order.Id
                        });
                    }

                    var orderSubmitDate = order.OrderUpdates.FirstOrDefault(x => x.Status == (int)OrderStatus.Placed);

                    context.SubmitChanges();
                    EmailManager.Send(
                        user,
                        this.repository,
                        EmailType.EmailResetPassword,
                        new Dictionary<string, string> { 
                        { "%order.id%", order.Id.ToString() }, 
                        { "%order.user%", order.User.Name }, 
                        { "%order.date%", orderSubmitDate != null ? orderSubmitDate.DateTime.ToString("F", russianCulture) : string.Empty } 
                    });
                }
            }

            return this.RedirectToAction("Orders");
        }
    }
}