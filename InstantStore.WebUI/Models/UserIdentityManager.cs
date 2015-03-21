using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.Models
{
    public class UserIdentityManager
    {
        private static readonly Dictionary<string, Guid> loggedInUsers = new Dictionary<string, Guid>();
        private const string UserCookie = @"_ssid";

        public static Guid? GetActiveUserId(HttpCookieCollection cookies)
        {
            if (!cookies.AllKeys.Contains(UserCookie, StringComparer.Ordinal))
            {
                return null;
            }

            string sessionId = cookies[UserCookie].Value;
            Guid userId;
            return !string.IsNullOrWhiteSpace(sessionId) && loggedInUsers.TryGetValue(sessionId, out userId) ? userId : (Guid?) null;
        }

        public static User GetActiveUser(HttpRequestBase httpRequest, IRepository repository)
        {
            var userId = GetActiveUserId(httpRequest.Cookies);
            return userId != null ? repository.GetUser(userId.Value) : null;
        }

        public static void ResetUser(HttpRequestBase httpRequest, HttpResponseBase httpResponse)
        {
            if (httpRequest.Cookies.AllKeys.Contains(UserCookie, StringComparer.Ordinal))
            {
                var sessionId = httpRequest.Cookies[UserCookie].Value;
                if (string.IsNullOrWhiteSpace(sessionId) && loggedInUsers.ContainsKey(sessionId))
                {
                    loggedInUsers.Remove(sessionId);
                }

                if (httpResponse.Cookies.AllKeys.Contains(UserCookie, StringComparer.Ordinal))
                {
                    httpResponse.Cookies.Remove(UserCookie);
                }
            }
        }

        public static void AddUserSession(HttpResponseBase httpResponse, User user)
        {
            var sessionId = Guid.NewGuid().ToString();
            var sessionIdCookie = new HttpCookie(UserCookie, sessionId);
            httpResponse.Cookies.Add(sessionIdCookie);
            loggedInUsers.Add(sessionId, user.Id);
        }
    }
}