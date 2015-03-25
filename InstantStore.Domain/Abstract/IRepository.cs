﻿using InstantStore.Domain.Concrete;
using InstantStore.Domain.Entities;
using System;
using System.Collections.Generic;

namespace InstantStore.Domain.Abstract
{
    public interface IRepository
    {
        IEnumerable<InstantStore.Domain.Entities.Product> Products { get; }

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
    }
}
