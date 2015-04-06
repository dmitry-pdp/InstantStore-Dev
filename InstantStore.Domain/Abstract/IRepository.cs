using InstantStore.Domain.Concrete;
using System;
using System.Collections.Generic;
using System.Data.Linq;

namespace InstantStore.Domain.Abstract
{
    public interface IRepository
    {
        //IEnumerable<InstantStore.Domain.Entities.Product> Products { get; }

        Setting Settings { get; }

        void Update(Setting settings);

        void AddFeedback(Feedback feedback);

        void AddUser(User user);

        User Login(string userName, string password);

        User GetUser(Guid id);

        IList<User> GetUsers(Func<User, bool> condition);

        void ActivateUser(Guid userId);

        void UnblockUser(Guid userId);

        void BlockUser(Guid userId);

        void UpdateUser(User user);

        IList<Currency> GetCurrencies();

        void AddCurrency(string text);

        void DeleteCurrency(Guid id);

        IList<ExchangeRate> GetExchangeRates();

        void AddExchangeRate(ExchangeRate exchangeRate);

        void UpdateExchangeRate(ExchangeRate exchangeRate);

        void DeleteExchangeRate(Guid id);

        IList<PropertyTemplate> GetTemplates();

        Guid AddNewTemplate(PropertyTemplate propertyTemplate);

        PropertyTemplate GetTemplateById(Guid id);

        void DeleteTemplate(Guid id);

        IList<CustomProperty> GetPropertiesForTemplate(Guid id);

        void UpdateTemplate(PropertyTemplate propertyTemplate, IList<CustomProperty> customProperties, bool forceUpdate);

        Guid AddNewCustomProperty(CustomProperty customProperty);

        void DeleteCustomProperty(Guid id);

        IList<ContentPage> GetPages(Guid? parentId);

        ContentPage GetPageById(Guid id);

        Guid NewPage(ContentPage contentPage);

        Guid NewCategory(Category category);

        Guid NewProduct(Product product);

        Image GetImageById(Guid id);
    }
}
