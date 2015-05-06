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

        void UpdateCustomProperty(Guid id, string data);

        IList<CustomProperty> CreateAttributesForProduct(Guid productId, Guid templateId);

        IList<ContentPage> GetPages(Guid? parentId, Func<ContentPage, bool> filter);

        ContentPage GetPageById(Guid id);

        ContentPage GetPageByCategoryId(Guid id);

        ContentPage GetPageByProductId(Guid id);

        void DeletePage(Guid id);

        void ChangePagePosition(Guid id, bool movedown);

        Guid NewPage(ContentPage contentPage);

        void UpdateContentPage(ContentPage contentPage);

        Guid NewCategory(Category category);

        Category GetCategoryById(Guid id);

        void UpdateCategory(Category category);

        Guid NewProduct(Product product);

        Product GetProductById(Guid id);

        IList<Product> GetProductsForCategory(Guid categoryId, int offset, int count);

        int GetProductsCountForCategory(Guid id);

        IList<Product> GetProductsByPopularity(int count);

        Image GetImageById(Guid id);

        ImageThumbnail GetImageThumbnailById(Guid id);

        Guid AddImage(Image image);

        Attachment GetAttachmentById(Guid id);

        Guid AddAttachment(Attachment attachment);

        void AssignImagesToProduct(Guid productId, IEnumerable<Guid> images);

        IList<Guid> GetImagesForProduct(Guid productId);

        void UpdateOrCreateNewProduct(Product productToUpdate, Guid parentId, IList<Guid> images, Guid? prototypeTemplateId, IList<CustomProperty> attributes);

        void AssignProductsToCategory(IList<Guid> products, Guid categoryId);
    }
}
