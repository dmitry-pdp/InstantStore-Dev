using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.WebUI.Resources;

namespace InstantStore.WebUI.ViewModels
{
    public class ControlPanelViewModel
    {
        private readonly IRepository repository;

        public ControlPanelViewModel(IRepository repository, ControlPanelPage page)
        {
            this.repository = repository;
            this.Items = new List<ControlPanelItemViewModel>();

            this.Initialize(page);
        }

        public List<ControlPanelItemViewModel> Items { get; private set; }

        private void Initialize(ControlPanelPage page)
        {
            this.Items.Add(new ControlPanelItemViewModel 
            { 
                Title = string.Format(StringResource.controlPanel_OrdersTemplate, 2, 100),
                ActionName = "Orders",
                IsActive = page == ControlPanelPage.Orders
            });

            var newUsersCount = this.repository.GetUsers(x => !x.IsActivated).Count;
            this.Items.Add(new ControlPanelItemViewModel
            {
                Title = StringResource.controlPanel_UsersTemplate,
                Badge = newUsersCount > 0 ? newUsersCount.ToString() : null,
                ActionName = "Users",
                IsActive = page == ControlPanelPage.Users
            });

            this.Items.Add(new ControlPanelItemViewModel
            {
                ActionName = "Offers",
                Title = "Скидки",
                IsActive = page == ControlPanelPage.Offers
            });

            this.Items.Add(new ControlPanelItemViewModel
            {
                ActionName = "Pages",
                Title = StringResource.admin_Pages,
                IsActive = page == ControlPanelPage.Pages
            });

            this.Items.Add(new ControlPanelItemViewModel
            {
                Title = StringResource.admin_CurrencyPage,
                ActionName = "Currency",
                IsActive = page == ControlPanelPage.Currency
            });

            this.Items.Add(new ControlPanelItemViewModel
            {
                Title = "Шаблоны",
                ActionName = "Templates",
                IsActive = page == ControlPanelPage.Templates
            });

            this.Items.Add(new ControlPanelItemViewModel
            {
                Title = "Настройки",
                ActionName = "Settings",
                IsActive = page == ControlPanelPage.Settings
            });
        }
    }

    public class ControlPanelItemViewModel
    {
        public string Title { get; set; }

        public string Badge { get; set; }

        public string ActionName { get; set; }

        public bool IsActive { get; set; }
    }

    public enum ControlPanelPage
    {
        Orders,
        Users,
        Currency,
        Settings,
        Offers,
        Pages,
        Templates
    }
}