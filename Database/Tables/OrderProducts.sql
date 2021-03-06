﻿CREATE TABLE [dbo].[OrderProducts]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [OrderId] UNIQUEIDENTIFIER NOT NULL, 
    [ProductId] UNIQUEIDENTIFIER NOT NULL, 
    [Count] INT NOT NULL, 

    [Price] DECIMAL NOT NULL, 
    [PriceCurrencyId] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [FK_OrderProducts_ToTableOrders] FOREIGN KEY ([OrderId]) REFERENCES [Order]([Id]), 
    CONSTRAINT [FK_OrderProducts_ToTableProducts] FOREIGN KEY ([ProductId]) REFERENCES [Product]([VersionId]), 
    CONSTRAINT [FK_OrderProducts_ToTableCurrency] FOREIGN KEY ([PriceCurrencyId]) REFERENCES [Currency]([Id])
)
