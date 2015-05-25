using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels.Factories
{
    public static class UserViewModelFactory
    {
        public static UserViewModelBase CreateUserViewModel(User user)
        {
            return new UserViewModelBase
            {
                Name = user.Name,
                City = user.City,
                Company = user.Company,
                Email = user.Email,
                Phonenumber = user.Phonenumber
            };
        }
    }
}