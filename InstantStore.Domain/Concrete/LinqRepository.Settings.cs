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
        EmailSettings_EmailAdmin,
        EmailSettings_EnableSSL,
        MetaTags_Description,
        MetaTags_Keywords,
        MetaTags_Copyright,
        MetaTags_Robots
    }

    public partial class LinqRepository
    {
        private static Dictionary<SettingsKey, string> settingsCache = new Dictionary<SettingsKey, string>();

        public string GetSettings(SettingsKey key)
        {
            string result;
            if (settingsCache.TryGetValue(key, out result))
            {
                return result;
            }
            else
            {
                using (var context = new InstantStoreDataContext())
                {
                    var keyString = key.ToString();
                    var setting = context.Settings.FirstOrDefault(x => x.Key == keyString);
                    result = setting != null ? setting.Value : null;
                }

                settingsCache[key] = result;
            }

            return result;
        }

        public void SetSettings(SettingsKey key, string value)
        {
            using(var context = new InstantStoreDataContext())
            {
                value = value ?? string.Empty;
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

            settingsCache[key] = value;
        }
    }
}
