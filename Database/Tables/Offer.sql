CREATE TABLE [dbo].[Offer] (
    [Id]                     UNIQUEIDENTIFIER NOT NULL,
    [Name]                   NVARCHAR (250)   NOT NULL,
    [ThresholdPriceValue]    DECIMAL (18, 4)  NOT NULL,
    [IsActive]               BIT              NOT NULL,
    [DiscountValue]          DECIMAL (18, 4)  NOT NULL,
    [CurrencyId]               UNIQUEIDENTIFIER NOT NULL,
    [MultiApply]             BIT              NOT NULL,
    [ThresholdMultiCurrency] BIT              NOT NULL,
    [Priority]               INT              NOT NULL,
    [Version]                INT              NOT NULL,
    [VersionId]              UNIQUEIDENTIFIER NOT NULL,
    [DiscountType]           INT              NOT NULL,
    PRIMARY KEY CLUSTERED ([VersionId] ASC),
    CONSTRAINT [FK_Offer_ToTableCurrency] FOREIGN KEY ([CurrencyId]) REFERENCES [dbo].[Currency] ([Id])
);

