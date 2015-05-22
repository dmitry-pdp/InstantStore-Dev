CREATE TABLE [dbo].[Order] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [Status]          INT              NOT NULL,
    [Comment]         NVARCHAR (MAX)   NULL,
    [TotalPrice]      NUMERIC (18)     NULL,
    [PriceCurrencyId] UNIQUEIDENTIFIER NULL,
    [UserId]          UNIQUEIDENTIFIER NOT NULL,
    [OfferId] UNIQUEIDENTIFIER NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Order_ToTableCurrency] FOREIGN KEY ([PriceCurrencyId]) REFERENCES [dbo].[Currency] ([Id]),
    CONSTRAINT [FK_Order_ToTableUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id]), 
    CONSTRAINT [FK_Order_ToTableOffer] FOREIGN KEY ([OfferId]) REFERENCES [Offer]([VersionId])
);

