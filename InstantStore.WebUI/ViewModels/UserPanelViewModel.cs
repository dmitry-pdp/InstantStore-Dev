using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.ViewModels
{
    public enum UserStatus
    {
        Active,
        Pending,
        Blocked
    }

    public class UserPanelViewModel
    {
        public string Title { get; set; }

        public UserStatus Status { get; set; }
    }
}