﻿using System;
using System.Web.Mvc;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Models;
using InstantStore.WebUI.ViewModels;
using InstantStore.WebUI.ViewModels.Factories;

namespace InstantStore.WebUI.Controllers
{
    [CustomAuthorization(false)]
    public partial class UserController : ControllerBase
    {
        public new ActionResult Profile()
        {
            this.Initialize(Guid.Empty, PageIdentity.UserProfile);

            return this.View(new UserViewModel
            {
                Name = this.currentUser.Name,
                City = this.currentUser.City,
                Company = this.currentUser.Company,
                Phonenumber = this.currentUser.Phonenumber
            });
        }

        [HttpPost]
        public new ActionResult Profile(UserViewModel userViewModel)
        {
            this.Initialize(Guid.Empty, PageIdentity.UserProfile);

            if (this.ModelState.IsValid)
            {
                this.currentUser.Name = userViewModel.Name;
                this.currentUser.City = userViewModel.City;
                this.currentUser.Company = userViewModel.Company;
                this.currentUser.Phonenumber = userViewModel.Phonenumber;
                this.repository.UpdateUser(this.currentUser);
                return this.RedirectToAction("Index", "Main");
            }

            return this.View(userViewModel);
        }

        public ActionResult ChangePassword()
        {
            this.Initialize(Guid.Empty, PageIdentity.Unknown);
            return this.View(new UserAuthViewModel());
        }

        [HttpPost]
        public ActionResult ChangePassword(UserAuthViewModel userViewModel)
        {
            this.Initialize(Guid.Empty, PageIdentity.Unknown);
            if (this.ModelState.IsValidField("Password") && this.ModelState.IsValidField("ConfirmPassword"))
            {
                this.repository.UpdatePassword(this.currentUser.Id, userViewModel.Password);
                return this.RedirectToAction("Profile");
            }

            return this.View(userViewModel);
        }
    }
}
