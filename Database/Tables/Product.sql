CREATE TABLE [dbo].[Product]
(
	[VersionId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [Id] UNIQUEIDENTIFIER NOT NULL, 
    [Name] NVARCHAR(250) NOT NULL, 
    [Description] NVARCHAR(MAX) NULL, 
    [MainImage] IMAGE NULL, 
    [CashAccepted] BIT NOT NULL, 
    [IsAvailable] BIT NOT NULL, 
    [PriceCurrencyId] UNIQUEIDENTIFIER NULL, 
    [PriceValueCash] NUMERIC(19, 4) NULL, 
    [PriceValueCashless] NUMERIC(19, 4) NULL, 
    [CustomAttributesTemplateId] UNIQUEIDENTIFIER NULL, 
    [Version] INT NOT NULL, 
    CONSTRAINT [FK_Product_ToTableCurrency] FOREIGN KEY ([PriceCurrencyId]) REFERENCES [Currency]([Id]), 
    CONSTRAINT [FK_Product_ToTablePropertyTemplate] FOREIGN KEY ([CustomAttributesTemplateId]) REFERENCES [PropertyTemplate]([Id])
)
