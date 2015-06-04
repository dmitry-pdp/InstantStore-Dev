using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.Domain.Concrete;

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
            EmailType.EmailNewUserNotification,
            EmailType.EmailOrderHasBeenPlaced
        };

        public static void Send(
            string to, 
            string from,
            string smtpServer,
            int smtpServerPort, 
            string smtpLogin,
            string smtpPassword,
            string subject, 
            string body,
            bool enableSSL)
        {
                MailMessage mail = new MailMessage(from, to, subject, body);
                SmtpClient client = new SmtpClient();
                client.Port = smtpServerPort;
                client.EnableSsl = enableSSL;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpLogin, smtpPassword);
                client.Host = smtpServer;
                client.Timeout = 5000;
                client.Send(mail);
        }

        public static void Send(User toUser, IRepository repository, EmailType emailType, IDictionary<string, string> emailProperties = null)
        {
            try
            {
                var fromEmail = repository.GetSettings(SettingsKey.EmailSettings_EmailFrom);
                var adminEmail = repository.GetSettings(SettingsKey.EmailSettings_EmailAdmin);
                var smtpServer = repository.GetSettings(SettingsKey.EmailSettings_SmtpServer);
                var smtpServerPort = repository.GetSettings(SettingsKey.EmailSettings_SmtpServerPort);
                var smtpServerLogin = repository.GetSettings(SettingsKey.EmailSettings_SmtpServerLogin);
                var smtpServerPassword = repository.GetSettings(SettingsKey.EmailSettings_SmtpServerPassword);
                var enableSSL = repository.GetSettings(SettingsKey.EmailSettings_EnableSSL) == "true";

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
                        foreach (var replacement in emailReplacements)
                        {
                            var value = replacement.Value(toUser);
                            emailSubject = emailSubject.Replace(replacement.Key, value);
                            emailBody = emailBody.Replace(replacement.Key, value);
                        }

                        if (emailProperties != null)
                        {
                            foreach (var property in emailProperties)
                            {
                                emailSubject = emailSubject.Replace(property.Key, property.Value);
                                emailBody = emailBody.Replace(property.Key, property.Value);
                            }
                        }

                        string to = isAdminEmail.Contains(emailType)
                            ? adminEmail
                            : toUser.Email;

                        if (string.IsNullOrWhiteSpace(to))
                        {
                            repository.LogError(null, DateTime.Now, "[code] email address of the recipient is null or empty.", null, null, null, null);
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
                            emailBody,
                            enableSSL);
                    }
                }
            }
            catch (Exception exception)
            {
                repository.LogError(exception, DateTime.Now, "[code] EmailManager.Send", null, null, null, null);
            }
        }
    }
}