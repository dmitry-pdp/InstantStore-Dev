using InstantStore.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace InstantStore.WebUI.Models.Helpers
{
    public static class Logging
    {
        public static void LogErrorMessage(this IRepository repository, string message, HttpRequestBase request)
        {
            if (repository == null)
            {
                Trace.TraceError("[Logging:LogErrorMessage] Repository is null. Skipping logging.");
                return;
            }

            if (request == null)
            {
                const string requestIsNull = "Warning: HttpRequest is null, no context information is available. Message: ";
                repository.LogError(requestIsNull + message, DateTime.Now, null, null, null, null, null);
                return;
            }

            repository.LogError(
                message,
                DateTime.Now,
                request.RawUrl,
                request.ServerVariables["REMOTE_ADDR"],
                request.ServerVariables["HTTP_USER_AGENT"],
                request.ServerVariables["ALL_RAW"],
                UserIdentityManager.GetActiveUserId(request.Cookies));
        }
    }
}