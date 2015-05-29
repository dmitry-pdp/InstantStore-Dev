using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantStore.Domain.Concrete
{
    public enum SettingsKey
    {
        HeaderHtml,
        FooterHtml,
        MainPageHtml,
        EmailNewUserRegistrationBody,
        EmailNewUserRegistrationSubject,
        EmailNewUserNotificationBody,
        EmailNewUserNotificationSubject,
        EmailNewUserActivationBody,
        EmailNewUserActivationSubject,
        EmailUserBlockedBody,
        EmailUserBlockedSubject,
        EmailResetPasswordBody,
        EmailResetPasswordSubject,
        EmailOrderHasBeenUpdatedBody,
        EmailOrderHasBeenUpdatedSubject,
        EmailOrderHasBeenPlacedBody,
        EmailOrderHasBeenPlacedSubject,
        EmailSettings_SmtpServer,
        EmailSettings_SmtpServerPort,
        EmailSettings_SmtpServerLogin,
        EmailSettings_SmtpServerPassword,
        EmailSettings_EmailFrom,
        EmailSettings_EmailAdmin
    }

    public partial class LinqRepository
    {
        public string GetSettings(SettingsKey key)
        { 
            using(var context = new InstantStoreDataContext())
            {
                var keyString = key.ToString();
                var setting = context.Settings.FirstOrDefault(x => x.Key == keyString);
                return setting != null ? setting.Value : null;
            }
        }

        public void SetSettings(SettingsKey key, string value)
        {
            using(var context = new InstantStoreDataContext())
            {
                var keyString = key.ToString();
                var setting = context.Settings.FirstOrDefault(x => x.Key == keyString);
                if (setting != null)
                {
                    setting.Value = value;
                }
                else
                {
                    context.Settings.InsertOnSubmit(new Setting
                    {
                        Key = keyString,
                        Value = value
                    });
                }

                context.SubmitChanges();
            }
        }
    }
}
