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

        public ControlPanelViewModel(IRepository repository)
        {
            this.repository = repository;
            this.Items = new List<ControlPanelItemViewModel>();

            this.Initialize();
        }

        public List<ControlPanelItemViewModel> Items { get; private set; }

        private void Initialize()
        {
            this.Items.Add(new ControlPanelItemViewModel 
            { 
                Title = string.Format(StringResource.controlPanel_OrdersTemplate, 2, 100)
            });

            var newUsersCount = this.repository.GetUsers(x => !x.IsActivated).Count;
            this.Items.Add(new ControlPanelItemViewModel
            {
                Title = StringResource.controlPanel_UsersTemplate,
                Badge = newUsersCount > 0 ? newUsersCount.ToString() : null,
                IsActive = true
            });

            this.Items.Add(new ControlPanelItemViewModel
            {
                Title = "Скидки"
            });

            this.Items.Add(new ControlPanelItemViewModel
            {
                Title = "Страницы"
            });

            this.Items.Add(new ControlPanelItemViewModel
            {
                Title = "Валюта"
            });

            this.Items.Add(new ControlPanelItemViewModel
            {
                Title = "Шаблоны"
            });

            this.Items.Add(new ControlPanelItemViewModel
            {
                Title = "Настройки"
            });
        }
    }

    public class ControlPanelItemViewModel
    {
        public string Title { get; set; }

        public string Badge { get; set; }

        public bool IsActive { get; set; }
    }
}