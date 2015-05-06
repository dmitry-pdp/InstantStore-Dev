﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.WebUI.Resources;
using InstantStore.Domain.Concrete;

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
            var mainMenuViewModel = new MainMenuViewModel { HasExit = true, Menu = new List<MenuItemViewModel>() };

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

        public static BreadcrumbViewModel CreateBreadcrumb(this IRepository repository, Guid? pageId)
        {
            var viewModel = new BreadcrumbViewModel();

            if (pageId == null || pageId == Guid.Empty)
            {
                viewModel.Add(new BreadcrumbItemViewModel { Name = StringResource.admin_HomeShort });
                return viewModel;
            }

            Guid pageIdentity = pageId.Value;
            while (pageIdentity != Guid.Empty)
            {
                var contentPage = repository.GetPageById(pageIdentity);
                if (contentPage == null)
                {
                    throw new ApplicationException("Invalid.Database.State");
                }

                viewModel.Insert(0, new BreadcrumbItemViewModel { 
                    Name = contentPage.Name,
                    Link = pageIdentity != pageId.Value 
                        ? new NavigationLink { PageId = pageIdentity }
                        : null,
                    IsActive = pageIdentity == pageId.Value
                });

                pageIdentity = contentPage.ParentId ?? Guid.Empty;
            }

            return viewModel;
        }

        public static NavigationMenuViewModel CreateNavigationMenu(this IRepository repository, Guid? pageId)
        {
            var viewModel = new NavigationMenuViewModel();

            if (pageId == null || pageId == Guid.Empty)
            {
                return null;
            }

            var contentPage = repository.GetPageById(pageId.Value);
            if (contentPage == null)
            {
                throw new ApplicationException("Invalid.Database.State");
            }

            var parentId = contentPage.ParentId;

            viewModel.BackLink = new NavigationLink { ActionName = "Page", PageId = parentId ?? Guid.Empty };
            viewModel.Items = new List<NavigationItemViewModel>();

            var siblingPages = repository.GetPages(parentId, null).Where(page => page.CategoryId != null).OrderBy(page => page.Position);
            if (siblingPages == null || !siblingPages.Any())
            {
                throw new ApplicationException("Invalid.Database.State");
            }

            List<NavigationItemViewModel> parentCategoryList = null;
            List<NavigationItemViewModel> childCategoryList = null;
            
            var childContentPages = repository.GetPages(pageId, null).Where(page => page.CategoryId != null);
            if (childContentPages != null && childContentPages.Any())
            {
                childCategoryList = childContentPages.OrderBy(page => page.Position).Select(page => new NavigationItemViewModel
                {
                    Name = page.Name,
                    Glyph = "glyphicon glyphicon-chevron-right",
                    Link = new NavigationLink { PageId = page.Id }
                })
                .ToList();
            }
            else if (parentId != null && parentId != Guid.Empty)
            {
                var parentContentPage = repository.GetPageById(parentId.Value);
                if (parentContentPage != null)
                {
                    var parentPagesList = repository.GetPages(parentContentPage.ParentId, null).Where(page => page.CategoryId != null);
                    if (parentPagesList != null && parentPagesList.Any())
                    {
                        parentCategoryList = parentPagesList.OrderBy(page => page.Position).Select(page => new NavigationItemViewModel
                        {
                            Name = page.Name,
                            Link = new NavigationLink { PageId = page.Id }
                        })
                        .ToList();
                    }
                }
            }

            if (parentCategoryList != null)
            {
                foreach (var parentCategory in parentCategoryList)
                { 
                    viewModel.Items.Add(parentCategory);
                    if (parentCategory.Link.PageId == parentId)
                    {
                        foreach (var siblingPage in siblingPages)
                        {
                            viewModel.Items.Add(new NavigationItemViewModel
                            {
                                Name = siblingPage.Name,
                                Glyph = "glyphicon glyphicon-chevron-right",
                                Link = new NavigationLink { PageId = siblingPage.Id },
                                IsActive = siblingPage.Id == contentPage.Id
                            });
                        }
                    }
                }
            }
            else
            {
                foreach (var siblingPage in siblingPages)
                {
                    bool current = siblingPage.Id == contentPage.Id;
                    viewModel.Items.Add(new NavigationItemViewModel
                    {
                        Name = siblingPage.Name,
                        Link = new NavigationLink { PageId = siblingPage.Id },
                        IsActive = current
                    });

                    if (current && childCategoryList != null)
                    {
                        childCategoryList.ForEach(item => viewModel.Items.Add(item));
                    }
                }
            }

            return viewModel;
        }

        private static List<MenuItemViewModel> CreateItems(IRepository repository, Guid? parentId, int level, Guid current)
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

            items.ForEach(item => item.Items = CreateItems(repository, item.Link.PageId, level + 1, current));
            return items;
        }
    }
}