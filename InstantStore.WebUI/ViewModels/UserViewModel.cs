using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using InstantStore.Domain.Abstract;
using InstantStore.WebUI.Resources;

namespace InstantStore.WebUI.ViewModels
{
    public class UserViewModel
    {
        public UserViewModel()
        {
        }

        public UserViewModel(IRepository repository, Guid id)
        {
            var user = repository.GetUser(id);
            if (user != null)
            {
                this.Name = user.Name;
                this.Email = user.Email;
                this.Company = user.Company;
                this.Phonenumber = user.Phonenumber;
                this.City = user.City;
            }
        }

        [Display(ResourceType = typeof(StringResource), Name = "form_Contact_Name")]
        [Required(ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_NameErrorRequired")]
        [StringLength(100, MinimumLength = 3, ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_NameErrorLength")]
        public string Name { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "form_Reg_Email")]
        [Required(ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "form_Reg_EmailInvalid")]
        [EmailAddress(ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "form_Reg_EmailInvalid")]
        public string Email { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "form_Reg_CompanyName")]
        [Required(ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_CompanyErrorRequired")]
        [StringLength(250, MinimumLength = 1, ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_CompanyErrorLength")]
        public string Company { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "form_Reg_TelNum")]
        [RegularExpression(@"^(?:(?:\(?(?:00|\+)([1-4]\d\d|[1-9]\d?)\)?)?[\-\.\ \\\/]?)?((?:\(?\d{1,}\)?[\-\.\ \\\/]?){0,})(?:[\-\.\ \\\/]?(?:#|ext\.?|extension|x)[\-\.\ \\\/]?(\d+))?$", ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_PhoneNumberInvalid")]
        [StringLength(50, ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_PhoneNumberLength")]
        public string Phonenumber { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "form_Reg_Region")]
        [Required(ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_CityErrorRequired")]
        [StringLength(100, MinimumLength = 3, ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_CityErrorLength")]
        public string City { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "form_Reg_Pwd")]
        [DataType(DataType.Password)]
        [Required(ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_PasswordErrorRequired")]
        [StringLength(50, MinimumLength = 3, ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_PasswordErrorLength")]
        public string Password { get; set; }

        [Display(ResourceType = typeof(StringResource), Name = "form_Reg_ConfirmPwd")]
        [DataType(DataType.Password)]
        [Required(ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_PasswordErrorRequired")]
        [Compare("Password", ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_PasswordErrorMismatch")]
        [StringLength(50, MinimumLength = 3, ErrorMessageResourceType = typeof(StringResource), ErrorMessageResourceName = "user_PasswordErrorLength")]
        public string ConfirmPassword { get; set; }
    }
}