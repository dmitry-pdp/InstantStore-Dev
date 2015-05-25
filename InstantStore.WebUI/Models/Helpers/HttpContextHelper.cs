using InstantStore.Domain.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.Models
{
    public static class HttpContextHelper
    {
        private static string userKey = @"HttpContextUser";

        public static User CurrentUser(this HttpContextBase context)
        {
            return context != null ? context.Items[userKey] as User : null;
        }

        public static void AssignUser(this HttpContextBase context, User user)
        {
            if (context != null) 
            {
                context.Items[userKey] = user;
            }
        }
    }
}