CREATE TABLE [dbo].[ProductToCategory] (
    [Id]         UNIQUEIDENTIFIER NOT NULL,
    [ProductId]  UNIQUEIDENTIFIER NOT NULL,
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    [UpdateTime] DATETIME         NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ProductToCategory_ToCategory] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[ContentPage] ([Id]),
    CONSTRAINT [FK_ProductToCategory_ToProduct] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([VersionId])
);

