using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.Models;
using InstantStore.WebUI.ViewModels.Factories;
using InstantStore.WebUI.Resources;

namespace InstantStore.WebUI.Controllers
{
    public partial class AdminController
    {
        private static Dictionary<char, UserStatus> userTabMap = new Dictionary<char, UserStatus>
        {
            {'a', UserStatus.Active },
            {'n', UserStatus.New },
            {'b', UserStatus.Blocked }
        };

        private static Dictionary<UserStatus, string> userTabLocalizationMap = new Dictionary<UserStatus, string>
        {
            { UserStatus.Active, StringResource.admin_Users_ActiveTab },
            { UserStatus.New, StringResource.admin_Users_NewTab },
            { UserStatus.Blocked, StringResource.admin_Users_BlockedTab }
        };

        public ActionResult Users(char t = 'n', int o = 0, int c = 50)
        {
            UserStatus status;
            if (!userTabMap.TryGetValue(t, out status))
            {
                status = UserStatus.Active;
            }

            using (var context = new InstantStoreDataContext())
            {
                bool hasNewUsers = context.Users.Any(user => !user.IsActivated);
                status = status == UserStatus.New && !hasNewUsers ? UserStatus.Active : status;

                Func<User, bool> selector = (User user) => (status == UserStatus.New ? !user.IsActivated : (status == UserStatus.Blocked ? user.IsBlocked : (user.IsActivated && !user.IsBlocked))) && !user.IsAdmin;
                this.ViewData["UsersTableViewModel"] = new TableViewModel
                {
                    Header = new List<TableCellViewModel>
                    {
                        new TableCellViewModel(StringResource.form_Contact_Name),
                        new TableCellViewModel(StringResource.form_Reg_Region),
                        new TableCellViewModel(StringResource.form_Reg_CompanyName)
                    },
                    Rows = context.Users
                        .Where(selector)
                        .OrderBy(user => user.Name)
                        .Skip(o)
                        .Take(c)
                        .Select(ConvertUserToTableRow)
                        .ToList(),
                    RowClickAction = new NavigationLink("User"),
                    Pagination = new PaginationViewModel(c, o, context.Users.Count(selector))
                    {
                        Link = new NavigationLink("Users", "Admin")
                        {
                            Parameters = new { t = t }
                        }
                    }
                };

                this.ViewData["UsersHeaderViewModel"] = new TabControlViewModel
                {
                    Tabs = userTabMap
                    .Where(x => hasNewUsers || x.Value != UserStatus.New)
                    .Select(key => CreateUserStatusHeader(key, t)).ToList()
                };
            }

            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Users);
            return View();
        }

        private TableRowViewModel ConvertUserToTableRow(User user)
        { 
            return new TableRowViewModel
            {
                Id = user.Id.ToString(),
                Cells = new List<TableCellViewModel>
                {
                    new TableCellViewModel(user.Name),
                    new TableCellViewModel(user.City),
                    new TableCellViewModel(user.Company)
                }
            };
        }

        private BreadcrumbItemViewModel CreateUserStatusHeader(KeyValuePair<char, UserStatus> data, char current)
        {
            return new BreadcrumbItemViewModel
            {
                IsActive = data.Key == current,
                Name = userTabLocalizationMap[data.Value],
                Link = new NavigationLink("Users", "Admin") { Parameters = new { t = data.Key } }
            };
        }

        public new ActionResult User(Guid id, bool? activate, bool? unblock, bool? block)
        {
            this.ViewData["SettingsViewModel"] = this.settingsViewModel;
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Users);
            this.ViewData["UsersListViewModel"] = new UsersListViewModel(this.repository, id);

            var user = this.repository.GetUser(id);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            if (activate != null && activate.Value)
            {
                this.repository.ActivateUser(id);
                EmailManager.Send(user, this.repository, EmailType.EmailNewUserActivation);
                return this.RedirectToAction("Users");
            }
            if (unblock != null && unblock.Value)
            {
                this.repository.UnblockUser(id);
                EmailManager.Send(user, this.repository, EmailType.EmailNewUserActivation);
                return this.RedirectToAction("Users");
            }
            if (block != null && block.Value)
            {
                this.repository.BlockUser(id);
                EmailManager.Send(user, this.repository, EmailType.EmailUserBlocked);
                return this.RedirectToAction("Users");
            }

            return this.View(new UserProfileViewModel(this.repository, id));
        }

        [HttpPost]
        [ValidateInput(false)]
        public new ActionResult User(UserProfileViewModel userProfileViewModel)
        {
            if (this.ModelState.IsValid)
            {
                var user = this.repository.GetUser(userProfileViewModel.Id);
                user.Name = userProfileViewModel.Name;
                user.Company = userProfileViewModel.Company;
                user.City = userProfileViewModel.City;
                user.Email = userProfileViewModel.Email;
                user.Phonenumber = userProfileViewModel.Phonenumber;
                user.IsPaymentCash = string.Equals(userProfileViewModel.PaymentType, "cash", StringComparison.OrdinalIgnoreCase);
                user.DefaultCurrencyId = userProfileViewModel.Currency;
                user.Comments = userProfileViewModel.Comments;
                this.repository.UpdateUser(user);

                return this.RedirectToAction("Users", "Admin");
            }

            this.ViewData["UsersListViewModel"] = new UsersListViewModel(this.repository, userProfileViewModel.Id);
            return this.View(new UserProfileViewModel(this.repository, userProfileViewModel.Id));
        }

        [HttpPost]
        public ActionResult ResetPassword(Guid id)
        {
            var user = this.repository.GetUser(id);
            if (user == null)
            {
                return this.HttpNotFound();
            }

            var guid = Guid.NewGuid().ToString();
            var newPassword = guid.Substring(guid.LastIndexOf('-') + 1);
            this.repository.ResetPassword(id, newPassword);
            EmailManager.Send(user, this.repository, EmailType.EmailResetPassword, new Dictionary<string, string> { { "%password%", newPassword } });
            return this.Json(new { status = "OK" });
        }
    }
}