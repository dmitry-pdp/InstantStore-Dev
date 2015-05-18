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
        public static UserPanelViewModel CreatePanelForUser(User user)
        {

            return user == null ? null : new UserPanelViewModel
            {
                Title = string.Format(StringResource.userInfo_HelloUserName, user.Name),
                Status = GetStatus(user)
            };
        }

        private static UserStatus GetStatus(User user)
        {
            if (user.IsBlocked)
            {
                return UserStatus.Blocked;
            } 
            else if (user.IsActivated)
            {
                return UserStatus.Active;
            }
            else
            {
                return UserStatus.Pending;
            }
        }

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