using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.WebUI.Resources;
using System.Web.Mvc;

namespace InstantStore.WebUI.ViewModels
{
    /// <summary>
    /// Admin settings user profile.
    /// </summary>
    public class UserProfileViewModel : UserViewModelBase
    {
        public UserProfileViewModel()
        {
        }

        public UserProfileViewModel(IRepository repository, Guid id)
        {
            var user = repository.GetUser(id);
            if (user != null)
            {
                this.Id = id;
                this.Name = user.Name;
                this.Email = user.Email;
                this.Company = user.Company;
                this.Phonenumber = user.Phonenumber;
                this.City = user.City;

                this.PaymentTypes = new List<SelectListItem>
                {
                    new SelectListItem()
                    {
                        Text = StringResource.PaymentType_Cash,
                        Value = "cash",
                        Selected = user.IsPaymentCash
                    },
                    new SelectListItem()
                    {
                        Text = StringResource.PaymentType_Cashless,
                        Value = "cashless",
                        Selected = !user.IsPaymentCash
                    }
                };

                this.Currencies = repository.GetCurrencies().Select(x => 
                {
                    return new SelectListItem()
                    {
                        Text = x.Text,
                        Value = x.Id.ToString(),
                        Selected = x.Id == user.DefaultCurrencyId
                    };
                }).ToList();
            }
        }

        [Display(ResourceType = typeof(StringResource), Name = "admin_UserPaymentType")]
        [Required(ErrorMessage="Payment type is not set")]
        public string PaymentType { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "admin_UserPaymentCurrency")]
        [Required(ErrorMessage = "Currency is not set")]
        public Guid Currency { get; set; }

        [Required(ErrorMessage = "Id is empty.")]
        public Guid Id { get; set; }

        public IList<SelectListItem> PaymentTypes { get; private set; }

        public IList<SelectListItem> Currencies { get; private set; }
    }
}