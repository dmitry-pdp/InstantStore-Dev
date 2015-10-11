using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Models;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.ViewModels.Factories;
using InstantStore.WebUI.Resources;

namespace InstantStore.WebUI.Controllers
{
    public partial class AdminController
    {
        private static string mappingRules = "Email properties:\n\r" +
            "User name: %user.name%, " +
            "New password: %password%, " +
            "Order id: %order.id%, " +
            "Order date: %order.date%, " +
            "Order user name: %order.user%" +
            "";

        private static CategoryTreeItemViewModel settingsNavigationTree = new CategoryTreeItemViewModel
        {
            Id = Guid.Empty,
            Name = StringResource.admin_Settings,
            Items = new List<CategoryTreeItemViewModel>
            {
                new CategoryTreeItemViewModel("PagesGroup", StringResource.admin_SettingsNode_PagesGroup)
                {
                    Items = new List<CategoryTreeItemViewModel>
                    {
                        new CategoryTreeItemViewModel("PagesHeader", StringResource.admin_SettingsNode_PagesHeader),
                        new CategoryTreeItemViewModel("PagesFooter", StringResource.admin_SettingsNode_PagesFooter),
                        new CategoryTreeItemViewModel("MainContent", StringResource.admin_SettingsNode_PagesMainContent)
                    }
                },
                new CategoryTreeItemViewModel("GroupEmail", StringResource.admin_SettingsNode_EmailGroup)
                {
                    Items = new List<CategoryTreeItemViewModel>
                    {
                        new CategoryTreeItemViewModel("EmailNewUserRegistration", StringResource.admin_SettingsNode_EmailNewUserRegistration),
                        new CategoryTreeItemViewModel("EmailNewUserNotification", StringResource.admin_SettingsNode_EmailNewUserNotification),
                        new CategoryTreeItemViewModel("EmailNewUserActivation", StringResource.admin_SettingsNode_EmailNewUserActivation),
                        new CategoryTreeItemViewModel("EmailUserBlocked", StringResource.admin_SettingsNode_EmailUserBlock),
                        new CategoryTreeItemViewModel("EmailResetPassword", StringResource.admin_SettingsNode_EmailResetPassword),
                        new CategoryTreeItemViewModel("EmailOrderHasBeenUpdated", StringResource.admin_SettingsNode_EmailOrderUpdated),
                        new CategoryTreeItemViewModel("EmailOrderHasBeenPlaced", StringResource.admin_SettingsNode_EmailNewOrder)
                    }
                },
                new CategoryTreeItemViewModel("Feedback", StringResource.admin_SettingsNode_FeedbackGroup),
                new CategoryTreeItemViewModel("LostPages", StringResource.admin_LostPages)
            }
        };

        public ActionResult Settings(Guid? i = null, Guid? id = null, int c = 25, int o = 0)
        {
            if (i == null)
            {
                this.ViewData["SettingsViewModel"] = this.settingsViewModel;
                this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Settings);
                this.ViewData["SettingsMenuViewModel"] = settingsNavigationTree;
                this.ViewData["SettingsMenuSelection"] = id;
                return View();
            }
            else
            {
                CustomViewModel settingsViewModel = null;
                var item = i != null ? this.GetItemById(i.Value, settingsNavigationTree) : null;
                if (item == null || item.Id == Guid.Empty)
                {
                }
                else
                {
                    if (item.Key.StartsWith("Email"))
                    {
                        settingsViewModel = this.CreateEmailSettingsViewModel(item);
                    }
                    else
                    {
                        switch (item.Key)
                        {
                            case "PagesHeader":
                                settingsViewModel = this.CreateContentSettingsViewModel(
                                    StringResource.Settings_HeaderLabel, SettingsKey.HeaderHtml);
                                break;
                            case "PagesFooter":
                                settingsViewModel = this.CreateContentSettingsViewModel(
                                    StringResource.Settings_FooterLabel, SettingsKey.FooterHtml);
                                break;
                            case "MainContent":
                                settingsViewModel = this.CreateContentSettingsViewModel(
                                    StringResource.Settings_MainTextLabel, SettingsKey.MainPageHtml);
                                break;
                            case "Feedback":
                                using (var context = new InstantStoreDataContext())
                                {
                                    settingsViewModel = new TableViewModel
                                    {
                                        Title = StringResource.admin_SettingsNode_FeedbackGroup,
                                        Rows = context.Feedbacks
                                            .OrderByDescending(feedback => feedback.Submitted)
                                            .Skip(o)
                                            .Take(c)
                                            .ToList()
                                            .Select(feedback => new TableRowViewModel
                                            {
                                                Cells = new List<TableCellViewModel>
                                            {
                                                new TableCellViewModel(string.Format(StringResource.admin_FeedbackNameFormat, feedback.Name, feedback.Email, feedback.Submitted.ToString("F", new CultureInfo("ru-RU")))),
                                                new TableCellViewModel(feedback.Message)
                                            }
                                            })
                                            .ToList(),
                                        Pagination = new PaginationViewModel(c, o, context.Feedbacks.Count()),
                                        ViewName = "TableView"
                                    };
                                }
                                break;
                            case "GroupEmail":
                                settingsViewModel = this.CreatePropertyListViewModel(
                                    StringResource.admin_Settings_EmailConfiguration,
                                    new List<PropertyInfo> 
                                { 
                                    new PropertyInfo(
                                        SettingsKey.EmailSettings_SmtpServer.ToString(), 
                                        StringResource.admin_Settings_EmailSettings_SmtpServer, 
                                        this.repository.GetSettings(SettingsKey.EmailSettings_SmtpServer)),
                                    new PropertyInfo(
                                        SettingsKey.EmailSettings_EnableSSL.ToString(), 
                                        StringResource.admin_Settings_EnableSSL, 
                                        this.repository.GetSettings(SettingsKey.EmailSettings_EnableSSL)),
                                    new PropertyInfo(
                                        SettingsKey.EmailSettings_SmtpServerPort.ToString(), 
                                        StringResource.admin_Settings_SmtpServerPort, 
                                        this.repository.GetSettings(SettingsKey.EmailSettings_SmtpServerPort)),
                                    new PropertyInfo(
                                        SettingsKey.EmailSettings_SmtpServerLogin.ToString(), 
                                        StringResource.admin_Settings_SmtpLogin, 
                                        this.repository.GetSettings(SettingsKey.EmailSettings_SmtpServerLogin)),
                                    new PropertyInfo(
                                        SettingsKey.EmailSettings_SmtpServerPassword.ToString(), 
                                        StringResource.admin_Settings_SmtpPassword, 
                                        this.repository.GetSettings(SettingsKey.EmailSettings_SmtpServerPassword)),
                                    new PropertyInfo(
                                        SettingsKey.EmailSettings_EmailFrom.ToString(), 
                                        StringResource.admin_Settings_EmailSettings_EmailFrom, 
                                        this.repository.GetSettings(SettingsKey.EmailSettings_EmailFrom)),
                                    new PropertyInfo(
                                        SettingsKey.EmailSettings_EmailAdmin.ToString(), 
                                        StringResource.admin_Settings_EmailAdmin, 
                                        this.repository.GetSettings(SettingsKey.EmailSettings_EmailAdmin)),
                                });
                                break;

                            case "LostPages":
                                settingsViewModel = this.CreatePageRecoveryViewModel(this.repository, c, o, i.Value);
                                break;
                        }
                    }
                }

                if (settingsViewModel != null)
                {
                    //settingsViewModel.Id = i ?? Guid.Empty;
                    return View(settingsViewModel.ViewName, settingsViewModel);
                }
                else
                {
                    return null;
                }
            }
        }

        private CustomViewModel CreatePropertyListViewModel(string title, IList<PropertyInfo> properties)
        {
            return new PropertyListViewModel
            {
                Title = title,
                Properties = properties,
                ViewName = "PropertyListView",
                CustomText = mappingRules
            };
        }

        private CustomViewModel CreateEmailSettingsViewModel(CategoryTreeItemViewModel item)
        {
            SettingsKey
                subjectKey = (SettingsKey)Enum.Parse(typeof(SettingsKey), item.Key + "Subject"),
                bodyKey = (SettingsKey)Enum.Parse(typeof(SettingsKey), item.Key + "Body");
            
            return new TextViewModel
            {
                Title = item.Name,
                Subject = this.repository.GetSettings(subjectKey),
                Content = this.repository.GetSettings(bodyKey),
                HasSubject = true,
                HasRichText = false,
                SubjectLabel = StringResource.admin_Settings_EmailSubject,
                ContentLabel = StringResource.admin_Settings_EmailBody,
                ViewName = "TextView"
            };
        }

        private CustomViewModel CreateContentSettingsViewModel(string title, SettingsKey key)
        {
            return new TextViewModel
            {
                HasRichText = true,
                ViewName = "TextView",
                Title = title,
                Content = this.repository.GetSettings(key)
            };
        }

        private CustomViewModel CreatePageRecoveryViewModel(IRepository repository, int count, int offset, Guid i)
        {
            var pagesInGraph = GetChildrenPages(repository, null);
            var pagesNotInGraph = repository.GetAllPages().Where(x => x.ParentId == LinqRepository.TrashParentId || !pagesInGraph.Any(y => y.Id == x.Id));

            return new TableViewModel()
            {
                Title = StringResource.admin_LostPages,
                ViewName = "TableView",
                RowClickAction = new NavigationLink("Page", "Admin"),
                Header = new List<TableCellViewModel>
                {
                    //new TableCellViewModel(StringResource.pg_PwdRecovery_Submit),
                    new TableCellViewModel(StringResource.admin_Name)
                },
                Rows = pagesNotInGraph.Select(p => new TableRowViewModel
                {
                    Id = p.Id.ToString(),
                    Cells = new List<TableCellViewModel> 
                    { 
                        //new TableCellViewModel(new NavigationLink("RecoverPage", "Admin") { PageId = p.Id }),
                        new TableCellViewModel(p.Name)
                    }
                }).ToList()
            };
        }

        private IList<ContentPage> GetChildrenPages(IRepository repository, Guid? parentId)
        {
            var children = repository.GetPages(parentId, null);
            var result = new List<ContentPage>(children);
            foreach (var child in children)
            {
                result.AddRange(GetChildrenPages(repository, child.Id));
            }

            return result;
        }

        private CategoryTreeItemViewModel GetItemById(Guid id, CategoryTreeItemViewModel item)
        {
            if (item == null)
            {
                return null;
            }

            if (item.Id == id)
            {
                return item;
            }

            if (item.Items != null)
            {
                foreach(var child in item.Items)
                {
                    var result = GetItemById(id, child);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SettingsUpdate(string headerHtml, string mainDocumentHtml, string footerHtml)
        {
            if (settingsViewModel != null)
            {
                this.settingsViewModel.HeaderHtml = headerHtml;
                this.settingsViewModel.FooterHtml = footerHtml;
                this.settingsViewModel.MainDocumentHtml = mainDocumentHtml;

                this.settingsViewModel.ValidateAndSave();
            }

            return new RedirectResult("/");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Settings(Guid i, string t, TextViewModel data, PropertyListViewModel propertyList)
        {
            var item = i != null ? this.GetItemById(i, settingsNavigationTree) : null;
            if (item == null || item.Id == Guid.Empty)
            {
                return null;
            }

            if (t == "TextView")
            {
                if (item.Key.StartsWith("Email"))
                {
                    SettingsKey
                        subjectKey = (SettingsKey)Enum.Parse(typeof(SettingsKey), item.Key + "Subject"),
                        bodyKey = (SettingsKey)Enum.Parse(typeof(SettingsKey), item.Key + "Body");

                    this.repository.SetSettings(subjectKey, data.Subject);
                    this.repository.SetSettings(bodyKey, data.Content);
                }
                else
                {
                    switch (item.Key)
                    {
                        case "PagesHeader":
                            this.repository.SetSettings(SettingsKey.HeaderHtml, data.Content);
                            break;
                        case "PagesFooter":
                            this.repository.SetSettings(SettingsKey.FooterHtml, data.Content);
                            break;
                        case "MainContent":
                            this.repository.SetSettings(SettingsKey.MainPageHtml, data.Content);
                            break;
                    }
                }

                return this.RedirectToAction("Settings", new { id = i });
            }
            else if (t == "PropertyListView" && item.Key == "GroupEmail")
            {
                foreach(var property in propertyList.Properties)
                {
                    SettingsKey settingsKey;
                    if (Enum.TryParse<SettingsKey>(property.Key, out settingsKey))
                    {
                        this.repository.SetSettings(settingsKey, property.Value);
                    }
                }

                return this.RedirectToAction("Settings", new { id = i });
            }

            return null;
        }
    }
}