using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantStore.Domain.Concrete
{
    public partial class LinqRepository
    {
        public Setting Settings 
        { 
            get
            {
                using(var context = new InstantStoreDataContext())
                {
                    var settingsTable = context.Settings;
                    return settingsTable.FirstOrDefault();
                }
            }
        }

        public void Update(Setting settings)
        {
            using(var context = new InstantStoreDataContext())
            {
                var settingsTable = context.Settings;
                if (!settingsTable.Any())
                {
                    settings.Id = Guid.NewGuid();
                    settingsTable.InsertOnSubmit(settings);
                }
                else
                {
                    var existingSettings = settingsTable.First();
                    existingSettings.HeaderHtml = settings.HeaderHtml;
                    existingSettings.FooterHtml = settings.FooterHtml;
                    existingSettings.MainDescription = settings.MainDescription;
                }

                context.SubmitChanges();
            }
        }
    }
}
