using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.WebUI.Resources;

namespace InstantStore.WebUI.ViewModels.Factories
{
    public static class MenuViewModelFactory
    {
        public static MainMenuViewModel CreateDefaultMenu(IRepository repository, Guid current)
        {
            var mainMenuViewModel = new MainMenuViewModel();
            mainMenuViewModel.Menu = CreateItems(repository, null, 0, current);
            mainMenuViewModel.Menu.Insert(0, new MenuItemViewModel { Name = StringResource.admin_HomeShort, IsActive = current == Guid.Empty, Link = new NavigationLink { ActionName = "Index" } });
            return mainMenuViewModel;
        }

        public static MainMenuViewModel CreateAdminMenu(IRepository repository, ControlPanelPage page)
        {
            var newUsersCount = repository.GetUsers(x => !x.IsActivated).Count;
            var mainMenuViewModel = new MainMenuViewModel { Menu = new List<MenuItemViewModel>(), HasExit = true };

            mainMenuViewModel.Menu.Add(new MenuItemViewModel {
                Name = StringResource.controlPanel_UsersTemplate,
                Badge = newUsersCount > 0 ? newUsersCount.ToString() : null,
                Link = new NavigationLink { ActionName = "Users", ControllerName = "Admin" },
                Glyph = "glyphicon glyphicon-user",
                IsActive = page == ControlPanelPage.Users
            });

            mainMenuViewModel.Menu.Add(new MenuItemViewModel
            {
                Name = StringResource.admin_UsersOrdersAction,
                Badge = 10.ToString(),
                Link = new NavigationLink { ActionName = "Orders", ControllerName = "Admin" },
                Glyph = "glyphicon glyphicon-shopping-cart",
                IsActive = page == ControlPanelPage.Orders
            });

            mainMenuViewModel.Menu.Add(new MenuItemViewModel
            {
                Name = StringResource.admin_Pages,
                Link = new NavigationLink { ActionName = "Pages", ControllerName = "Admin" },
                Glyph = "glyphicon glyphicon-file",
                IsActive = page == ControlPanelPage.Pages
            });

            mainMenuViewModel.Menu.Add(new MenuItemViewModel
            {
                Name = StringResource.admin_CurrencyPage,
                Link = new NavigationLink { ActionName = "Currency", ControllerName = "Admin" },
                Glyph = "glyphicon glyphicon-usd",
                IsActive = page == ControlPanelPage.Currency
            });

            mainMenuViewModel.Menu.Add(new MenuItemViewModel
            {
                Name = "Шаблоны",
                Link = new NavigationLink { ActionName = "Templates", ControllerName = "Admin" },
                Glyph = "glyphicon glyphicon-list",
                IsActive = page == ControlPanelPage.Templates
            });

            mainMenuViewModel.Menu.Add(new MenuItemViewModel
            {
                Name = "Настройки",
                Link = new NavigationLink { ActionName = "Settings", ControllerName = "Admin" },
                Glyph = "glyphicon glyphicon-wrench",
                IsActive = page == ControlPanelPage.Settings
            });

            return mainMenuViewModel;
        }

        private static IList<MenuItemViewModel> CreateItems(IRepository repository, Guid? parentId, int level, Guid current)
        {
            var items = repository
                .GetPages(parentId, page => page.ShowInMenu)
                .OrderBy(p => p.Position)
                .Select(p => new MenuItemViewModel
                {
                    Link = new NavigationLink { PageId = p.Id },
                    Name = p.Name,
                    Level = level,
                    IsActive = p.Id == current
                }).ToList();

            foreach (var item in items)
            {
                item.Items = CreateItems(repository, item.Link.PageId, level + 1, current);
            }

            return items;
        }
    }
}