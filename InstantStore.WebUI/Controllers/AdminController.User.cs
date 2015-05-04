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

namespace InstantStore.WebUI.Controllers
{
    public partial class AdminController
    {
        public ActionResult Users()
        {
            this.ViewData["MainMenuViewModel"] = MenuViewModelFactory.CreateAdminMenu(repository, ControlPanelPage.Users);
            this.ViewData["ControlPanelViewModel"] = new ControlPanelViewModel(this.repository, ControlPanelPage.Users);
            this.ViewData["UsersListViewModel"] = new UsersListViewModel(this.repository);
            return this.Authorize() ?? View();
        }

        public ActionResult User(Guid id, bool? activate, bool? unblock, bool? block)
        {
            this.ViewData["UsersListViewModel"] = new UsersListViewModel(this.repository, id);
            if (activate != null && activate.Value)
            {
                this.repository.ActivateUser(id);
                return this.RedirectToAction("Users");
            }
            if (unblock != null && unblock.Value)
            {
                this.repository.UnblockUser(id);
                return this.RedirectToAction("Users");
            }
            if (block != null && block.Value)
            {
                this.repository.BlockUser(id);
                return this.RedirectToAction("Users");
            }

            return this.Authorize() ?? this.View(new UserProfileViewModel(this.repository, id));
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult User(UserProfileViewModel userProfileViewModel)
        {
            var nonAuthorizedResult = this.Authorize();
            if (nonAuthorizedResult != null)
            {
                return nonAuthorizedResult;
            }

            if (this.ModelState.IsValid)
            {
                var user = this.repository.GetUser(userProfileViewModel.Id);
                user.Name = userProfileViewModel.Name;
                user.Email = userProfileViewModel.Email;
                user.Company = userProfileViewModel.Company;
                user.City = userProfileViewModel.City;
                user.Phonenumber = userProfileViewModel.Phonenumber;
                user.IsPaymentCash = string.Equals(userProfileViewModel.PaymentType, "cash", StringComparison.OrdinalIgnoreCase);
                user.DefaultCurrencyId = userProfileViewModel.Currency;
                user.Comments = userProfileViewModel.Comments;
                this.repository.UpdateUser(user);
            }

            this.ViewData["UsersListViewModel"] = new UsersListViewModel(this.repository, userProfileViewModel.Id);
            return this.View(new UserProfileViewModel(this.repository, userProfileViewModel.Id));
        }
    }
}