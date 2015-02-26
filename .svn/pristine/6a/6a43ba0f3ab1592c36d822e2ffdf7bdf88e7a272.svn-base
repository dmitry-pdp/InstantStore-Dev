CREATE TABLE [dbo].[Products] (
    [ProductId]     INT             IDENTITY (1, 1) NOT NULL,
    [Name]          NVARCHAR (100)  COLLATE Cyrillic_General_CI_AI NOT NULL,
    [Description]   NTEXT           COLLATE Cyrillic_General_CI_AI DEFAULT ('') NOT NULL,
    [PriceCash]     DECIMAL (16, 2) DEFAULT ((0)) NOT NULL,
    [PriceCashless] DECIMAL (16, 2) DEFAULT ((0)) NOT NULL,
    [ProductSizeID] INT             DEFAULT ((1)) NOT NULL,
    PRIMARY KEY CLUSTERED ([ProductId] ASC),
    CONSTRAINT [FK_productSize] FOREIGN KEY ([ProductSizeID]) REFERENCES [dbo].[ProductSize] ([Id]) ON DELETE CASCADE
);



