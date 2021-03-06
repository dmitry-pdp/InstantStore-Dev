﻿using InstantStore.Domain.Concrete;
using InstantStore.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Linq;

namespace InstantStore.Domain.Abstract
{
    public interface IRepository
    {
        //IEnumerable<InstantStore.Domain.Entities.Product> Products { get; }
        void LogError(Exception exception, DateTime time, string requestUrl, string clientIp, string userAgent, string sessionId, Guid? userId);

        void LogError(string message, DateTime time, string requestUrl, string clientIp, string userAgent, string sessionId, Guid? userId);

        string GetSettings(SettingsKey key);

        void SetSettings(SettingsKey key, string value);

        void AddFeedback(Feedback feedback);

        void AddUser(User user);

        User Login(string userName, string password);

        User GetUser(Guid id);

        IList<User> GetUsers(Func<User, bool> condition);

        void ActivateUser(Guid userId);

        void UnblockUser(Guid userId);

        void BlockUser(Guid userId);

        void UpdateUser(User user);

        void ResetPassword(Guid userId, string newPassword);

        void UpdatePassword(Guid userId, string password);

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

        IList<CustomProperty> CreateAttributesForProduct(Guid templateId);

        IList<ContentPage> GetAllPages();

        IList<ContentPage> GetPages(Guid? parentId, Func<ContentPage, bool> filter);

        ContentPage GetPageById(Guid id);

        ContentPage GetPageByCategoryId(Guid id);

        ContentPage GetPageByProductId(Guid id);

        ContentPageTree GetCatalogTree();

        void DeletePage(Guid id);

        void ChangePagePosition(Guid id, bool movedown);

        Guid NewPage(ContentPage contentPage, IList<Guid> attachmentIds);

        void UpdateContentPage(ContentPage contentPage, IList<Guid> attachmentIds);

        IList<ContentPageAttachment> GetPageAttachments(Guid pageId);
        
        Guid NewCategory(Category category);

        Category GetCategoryById(Guid id);

        void UpdateCategory(Category category);

        IList<Category> GetPriorityCategories();

        Guid NewProduct(Product product);

        Product GetProductById(Guid id);

        IList<Tuple<string, Guid?, Product>> GetProductsForCategory(Guid categoryId, int offset, int count);

        int GetProductsCountForCategory(Guid id);

        IList<Product> GetProductsByPopularity(int count);

        void RemoveProductFromCategory(Guid categoryId, IList<Guid> productIdsToRemove);

        Image GetImageById(Guid id);

        ImageThumbnail GetImageThumbnailById(Guid id);

        Guid AddImage(Image image);

        Attachment GetAttachmentById(Guid id);

        Guid AddAttachment(Attachment attachment);

        void AssignImagesToProduct(Guid productId, IEnumerable<Guid> images);

        IList<Guid> GetImagesForProduct(Guid productId);

        void UpdateOrCreateNewProduct(Product productToUpdate, Guid parentId, IList<Guid> images, Guid? prototypeTemplateId, IList<CustomProperty> attributes, int position);

        void AssignProductsToCategory(IList<Guid> products, Guid categoryId);

        int GetProductPosition(Guid id, Guid parentId);

        // Orders

        int GetOrderItemsCount(User user);

        Order AddItemToCurrentOrder(User user, Guid productId, int count);

        Order GetActiveOrder(Guid userId);

        Order GetOrderById(Guid userId);

        IList<KeyValuePair<OrderProduct, Product>> GetProductsForOrder(Guid orderId);

        void DeleteOrderProduct(Guid orderProductId);

        void UpdateOrderProduct(OrderProduct orderProduct);

        void RemoveOrderProduct(Guid orderProductId);

        void SubmitOrder(Guid orderId);

        OrdersQueryResult GetOrdersWithStatus(IEnumerable<OrderStatus> statuses, Guid? userId, int offset, int count);

        IList<OrderUpdate> GetStatusesForOrder(Guid orderId);

        void AddProductsFromOrder(Guid orderId, User user);

        Guid CloneProduct(Guid productId, Guid parentId);

        void TrashPage(Guid id);
    }
    public struct OrdersQueryResult
    {
        public IList<Order> Result;

        public int MaxCount;
    }
}
