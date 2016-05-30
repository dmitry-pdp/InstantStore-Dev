using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Resources;
using InstantStore.WebUI.Models.Helpers;

namespace InstantStore.WebUI.ViewModels.Factories
{
    public enum PageIdentity
    {
        Unknown,
        UserProfile,
        Cart,
        History,
        Feedback
    }
    
    public static class MenuViewModelFactory
    {
        private static List<MenuItemViewModel> menuItemsCache = null;

        public static MainMenuViewModel CreateDefaultMenu(IRepository repository, Guid current, User user, PageIdentity page = PageIdentity.Unknown)
        {
            var mainMenuViewModel = new MainMenuViewModel { MetaMenu = new List<MenuItemViewModel>() };
            mainMenuViewModel.Menu = menuItemsCache ?? CreateItems(repository, null, 0);
            SelectActiveMenuItem(mainMenuViewModel.Menu, current);

            mainMenuViewModel.Menu.Insert(0, new MenuItemViewModel 
            { 
                Name = StringResource.admin_HomeShort, 
                IsActive = current == Guid.Empty && page == PageIdentity.Unknown,
                Link = new NavigationLink("Index", "Main")
            });
            
            if (user != null)
            {
                if (user.IsAdmin)
                {
                    mainMenuViewModel.MetaMenu.Add(new MenuItemViewModel
                    {
                        Glyph = "glyphicon glyphicon-cog",
                        Name = StringResource.admin_Dashboard,
                        Link = new NavigationLink { ActionName = "Pages", ControllerName = "Admin" }
                    });
                }
                else
                {
                    /*
                    mainMenuViewModel.MetaMenu.Add(new MenuItemViewModel
                    {
                        Glyph = "glyphicon glyphicon-briefcase",
                        Name = StringResource.nav_History,
                        Link = new NavigationLink { ActionName = "History", ControllerName = "User" },
                        IsActive = page == PageIdentity.History
                    });

                    var ordersCount = repository.GetOrderItemsCount(user);

                    mainMenuViewModel.MetaMenu.Add(new MenuItemViewModel
                    {
                        Glyph = "glyphicon glyphicon-shopping-cart",
                        Name = StringResource.nav_Orders,
                        Link = new NavigationLink { ActionName = "Orders", ControllerName = "User" },
                        Badge = ordersCount > 0 ? ordersCount.ToString() : null,
                        IsActive = page == PageIdentity.Cart
                    });
                    */

                    mainMenuViewModel.MetaMenu.Add(new MenuItemViewModel
                    {
                        Glyph = "glyphicon glyphicon-user",
                        Name = StringResource.nav_Profile,
                        Link = new NavigationLink { ActionName = "Profile", ControllerName = "User" },
                        IsActive = page == PageIdentity.UserProfile
                    });
                }
            }

            if (user == null || !user.IsAdmin)
            {
                mainMenuViewModel.MetaMenu.Add(new MenuItemViewModel
                {
                    Glyph = "glyphicon glyphicon-paperclip",
                    Name = StringResource.form_Contact_us,
                    Link = new NavigationLink { ActionName = "Feedback", ControllerName = "Main" },
                    IsActive = page == PageIdentity.Feedback
                });
            }

            if (user != null)
            {
                mainMenuViewModel.MetaMenu.Add(new MenuItemViewModel
                {
                    Glyph = "glyphicon glyphicon-log-out",
                    Name = StringResource.Logout,
                    Link = new NavigationLink { ActionName = "Logoff", ControllerName = "Main" }
                });
            }
            else
            {
                mainMenuViewModel.MetaMenu.Add(new MenuItemViewModel
                {
                    Glyph = "glyphicon glyphicon-log-in",
                    Name = StringResource.login_LoginAsUser,
                    Link = new NavigationLink { ActionName = "Login", ControllerName = "Main" }
                });
            }

            return mainMenuViewModel;
        }

        public static MainMenuViewModel CreateAdminMenu(IRepository repository, ControlPanelPage page)
        {
            var newUsersCount = repository.GetUsers(x => !x.IsActivated).Count;
            var mainMenuViewModel = new MainMenuViewModel { Menu = new List<MenuItemViewModel>(), MetaMenu = new List<MenuItemViewModel>() };

            mainMenuViewModel.MetaMenu.Add(new MenuItemViewModel {
                Glyph = "glyphicon glyphicon-log-out",
                Name = StringResource.admin_Nav_Exit,
                Link = new NavigationLink { ActionName= "Index", ControllerName = "Main" }
            });

            mainMenuViewModel.Menu.Add(new MenuItemViewModel {
                Name = StringResource.controlPanel_UsersTemplate,
                Badge = newUsersCount > 0 ? newUsersCount.ToString() : null,
                Link = new NavigationLink { ActionName = "Users", ControllerName = "Admin" },
                Glyph = "glyphicon glyphicon-user",
                IsActive = page == ControlPanelPage.Users
            });

            /*
            mainMenuViewModel.Menu.Add(new MenuItemViewModel
            {
                Name = StringResource.admin_UsersOrdersAction,
                Badge = repository.GetOrdersWithStatus(new [] { OrderStatus.Placed }, null, 0, -1).Result.Count.ToString(),
                Link = new NavigationLink { ActionName = "Orders", ControllerName = "Admin" },
                Glyph = "glyphicon glyphicon-shopping-cart",
                IsActive = page == ControlPanelPage.Orders
            });
            */

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
                Name = StringResource.admin_Offers,
                Link = new NavigationLink { ActionName = "Offers", ControllerName = "Admin" },
                Glyph = "glyphicon glyphicon glyphicon-star",
                IsActive = page == ControlPanelPage.Offers
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

        public static NavigationMenuViewModel CreateNavigationMenu(this IRepository repository, Guid? pageId, HttpRequestBase httpRequest)
        {
            var viewModel = new NavigationMenuViewModel 
            { 
                Title = StringResource.navbar_CategoriesTitle,
                Items = new List<NavigationItemViewModel>()
            };

            var catalogTree = repository.GetCatalogTree();
            if (catalogTree == null || catalogTree.Root == null || catalogTree.Root.Children == null)
            {
                repository.LogErrorMessage("[CreateNavigationMenu] Catalog tree is null or its root is null or empty.", httpRequest);
                return viewModel;
            }

            var catalogItems = catalogTree.Root.Children
                .Where(page => page != null && page.Page != null)
                .OrderBy(page => page.Page.Position);

            foreach (var catalogItem in catalogItems)
            { 
                bool isCurrentPage = catalogItem.Page.Id == pageId;

                viewModel.Items.Add(new NavigationItemViewModel
                {
                    Name = catalogItem.Page.Name,
                    IsActive = isCurrentPage,
                    Link = new NavigationLink { PageId = catalogItem.Page.Id }
                });

                if (isCurrentPage || catalogTree.LookupPage(pageId ?? Guid.Empty, catalogItem) != null)
                {
                    foreach(var catalogChildItem in catalogItem.Children.OrderBy(page => page.Page.Position))
                    {
                        viewModel.Items.Add(new NavigationItemViewModel
                        {
                            Name = catalogChildItem.Page.Name,
                            Glyph = "glyphicon glyphicon-chevron-right",
                            IsActive = catalogTree.LookupPage(pageId ?? Guid.Empty, catalogChildItem) != null,
                            Link = new NavigationLink { PageId = catalogChildItem.Page.Id }
                        });
                    }
                }
            }

            /*
            var siblingPages = repository.GetPages(parentId, null).Where(page => page.CategoryId != null).OrderBy(page => page.Position);
            if (siblingPages == null || !siblingPages.Any())
            {
                return viewModel;
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
            */
            return viewModel;
        }

        public static void ResetCache()
        {
            menuItemsCache = null;
        }

        private static void SelectActiveMenuItem(IList<MenuItemViewModel> items, Guid current)
        { 
            if (items == null)
            {
                return;
            }

            foreach(var item in items)
            {
                if (item.Link.PageId == current)
                {
                    item.IsActive = true;
                    break;
                }

                SelectActiveMenuItem(item.Items, current);
            }
        }

        private static List<MenuItemViewModel> CreateItems(IRepository repository, Guid? parentId, int level/*, Guid current*/)
        {
            var items = repository
                .GetPages(parentId, page => page.ShowInMenu)
                .OrderBy(p => p.Position)
                .Select(p => new MenuItemViewModel
                {
                    Link = new NavigationLink("Page", "Main") { PageId = p.Id },
                    Name = p.Name,
                    Level = level,
                    //IsActive = p.Id == current
                }).ToList();

            items.ForEach(item => item.Items = CreateItems(repository, item.Link.PageId, level + 1/*, current*/));
            return items;
        }
    }
}