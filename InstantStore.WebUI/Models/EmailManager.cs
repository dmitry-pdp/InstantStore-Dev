using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace InstantStore.WebUI.Models
{
    public enum EmailType
    {
        // email to the user when user is registered
        EmailNewUserRegistration,
        
        // email to admin when user has been registered
        EmailNewUserNotification,
        
        // email to the user when admin activates the user account
        EmailNewUserActivation,

        // email to the user when admin blocks the user account
        EmailUserBlocked,

        // email to the user when the password has been reset
        EmailResetPassword,

        // email to the user when the order has been updated
        EmailOrderHasBeenUpdated,

        // email to the admin when the order has been placed
        EmailOrderHasBeenPlaced,
    };

    public static class EmailManager
    {
        public static Dictionary<string, Func<User, string>> emailReplacements = new Dictionary<string, Func<User, string>>
        {
            { "%user.name%", (User u) => u.Name }
        };

        private static HashSet<EmailType> isAdminEmail = new HashSet<EmailType>
        {
            EmailType.EmailNewUserNotification
        };

        public static void Send(
            string to, 
            string from,
            string smtpServer,
            int smtpServerPort, 
            string smtpLogin,
            string smtpPassword,
            string subject, 
            string body)
        {
            MailMessage mail = new MailMessage(from, to, subject, body);
            SmtpClient client = new SmtpClient();
            client.Port = smtpServerPort;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(smtpLogin, smtpPassword);
            client.Host = smtpServer;
            client.Timeout = 5000;
            client.Send(mail);
        }

        public static void Send(User toUser, IRepository repository, EmailType emailType)
        {
            var fromEmail = repository.GetSettings(SettingsKey.EmailSettings_EmailFrom);
            var adminEmail = repository.GetSettings(SettingsKey.EmailSettings_EmailAdmin);
            var smtpServer = repository.GetSettings(SettingsKey.EmailSettings_SmtpServer);
            var smtpServerPort = repository.GetSettings(SettingsKey.EmailSettings_SmtpServerPort);
            var smtpServerLogin = repository.GetSettings(SettingsKey.EmailSettings_SmtpServerLogin);
            var smtpServerPassword = repository.GetSettings(SettingsKey.EmailSettings_SmtpServerPassword);

            if (!string.IsNullOrWhiteSpace(fromEmail) && !string.IsNullOrWhiteSpace(smtpServer) && !string.IsNullOrWhiteSpace(adminEmail))
            {
                var emailTypeString = emailType.ToString();
                SettingsKey emailSubjectKey, emailBodyKey;
                string emailSubject, emailBody;

                if (Enum.TryParse<SettingsKey>(emailTypeString + "Subject", out emailSubjectKey) && 
                    !string.IsNullOrWhiteSpace(emailSubject = repository.GetSettings(emailSubjectKey)) && 
                    Enum.TryParse<SettingsKey>(emailTypeString + "Body", out emailBodyKey) && 
                    !string.IsNullOrWhiteSpace(emailBody = repository.GetSettings(emailBodyKey)))
                {
                    foreach(var replacement in emailReplacements)
                    {
                        var value = replacement.Value(toUser);
                        emailSubject = emailSubject.Replace(replacement.Key, value);
                        emailBody = emailBody.Replace(replacement.Key, value);
                    }

                    string to = isAdminEmail.Contains(emailType)
                        ? adminEmail
                        : toUser.Email;

                    if (string.IsNullOrWhiteSpace(to))
                    {
                        // LOG error.
                        return;
                    }

                    Send(
                        to, 
                        fromEmail, 
                        smtpServer, 
                        int.Parse(smtpServerPort), 
                        smtpServerLogin, 
                        smtpServerPassword, 
                        emailSubject,
                        emailBody);
                }
            }
        }
    }
}