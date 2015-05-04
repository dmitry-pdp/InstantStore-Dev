CREATE TABLE [dbo].[Product] (
    [VersionId]                  UNIQUEIDENTIFIER NOT NULL,
    [Id]                         UNIQUEIDENTIFIER NOT NULL,
    [Name]                       NVARCHAR (250)   NOT NULL,
    [Description]                NVARCHAR (MAX)   NULL,
    [MainImageId]                UNIQUEIDENTIFIER            NULL,
    [CashAccepted]               BIT              NOT NULL,
    [IsAvailable]                BIT              NOT NULL,
    [PriceCurrencyId]            UNIQUEIDENTIFIER NULL,
    [PriceValueCash]             NUMERIC (19, 4)  NULL,
    [PriceValueCashless]         NUMERIC (19, 4)  NULL,
    [CustomAttributesTemplateId] UNIQUEIDENTIFIER NULL,
    [Version]                    INT              NOT NULL,
    PRIMARY KEY CLUSTERED ([VersionId] ASC),
    CONSTRAINT [FK_Product_ToTableCurrency] FOREIGN KEY ([PriceCurrencyId]) REFERENCES [dbo].[Currency] ([Id]),
    CONSTRAINT [FK_Product_ToTablePropertyTemplate] FOREIGN KEY ([CustomAttributesTemplateId]) REFERENCES [dbo].[PropertyTemplate] ([Id]), 
    CONSTRAINT [FK_Product_ToTableImage] FOREIGN KEY ([MainImageId]) REFERENCES [Image]([Id])
);

