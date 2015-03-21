CREATE TABLE [dbo].[ExchangeRate]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [CurrencyId] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [FK_ExchangeRate_ToTable] FOREIGN KEY ([Column]) REFERENCES [ToTable]([ToTableColumn])
)
