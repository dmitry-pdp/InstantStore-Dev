CREATE TABLE [dbo].[ExchangeRate] (
    [Id]               INT              NOT NULL,
    [FromCurrencyId] UNIQUEIDENTIFIER NOT NULL,
    [ToCurrencyId] UNIQUEIDENTIFIER NOT NULL,
    [ConversionRate]   FLOAT (53)       NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC), 
    CONSTRAINT [FK_ExchangeRate_FromCurrency] FOREIGN KEY ([FromCurrencyId]) REFERENCES [Currency]([Id]),
    CONSTRAINT [FK_ExchangeRate_ToCurrency] FOREIGN KEY ([ToCurrencyId]) REFERENCES [Currency]([Id]),
);

