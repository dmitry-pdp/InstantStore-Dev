using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantStore.Domain.Concrete
{
    public partial class LinqRepository
    {
        public void LogError(Exception exception, DateTime time, string requestUrl, string clientIp, string userAgent, string sessionId, Guid? userId)
        {
            try
            {
                using (var context = new InstantStoreDataContext())
                {
                    // Logging message first in case user data is broken.
                    context.ErrorLogs.InsertOnSubmit(new ErrorLog() {
                        Id = Guid.NewGuid(),
                        ExceptionText = exception.ToString(),
                        DateTime = time, 
                        UserId = userId,
                        SessionId = sessionId,
                        RequestUrl = requestUrl,
                        ClientIP = clientIp,
                        UserAgent = userAgent
                    });
                    context.SubmitChanges();
                }
            }
            catch(Exception ex)
            {
                string message = string.Format(
                    "Error occurred during logging the error: exception info {0}. " +
                    "Error data: innerException: {1}, time: {2}, requestUrl: {3}, clientIp: {4}, userAgent: {5}, sessionId: {6}.",
                    exception.ToString(), time, requestUrl, clientIp, userAgent, sessionId);

                Trace.TraceError(ex.ToString());
            }
        }
    }
}
