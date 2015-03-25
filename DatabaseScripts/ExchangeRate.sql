CREATE TABLE [dbo].[ExchangeRate] (
    [Id]                    UNIQUEIDENTIFIER NOT NULL,
    [FromCurrencyId]        UNIQUEIDENTIFIER NOT NULL,
    [ToCurrencyId]          UNIQUEIDENTIFIER NOT NULL,
    [ConversionRate]        FLOAT (53)       NULL,
    [ReverseConversionRate] FLOAT (53)       NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ExchangeRate_FromCurrency] FOREIGN KEY ([FromCurrencyId]) REFERENCES [dbo].[Currency] ([Id]),
    CONSTRAINT [FK_ExchangeRate_ToCurrency] FOREIGN KEY ([ToCurrencyId]) REFERENCES [dbo].[Currency] ([Id])
);

