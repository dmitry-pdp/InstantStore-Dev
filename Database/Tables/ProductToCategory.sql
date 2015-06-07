CREATE TABLE [dbo].[ProductToCategory] (
    [Id]         UNIQUEIDENTIFIER NOT NULL,
    [ProductId]  UNIQUEIDENTIFIER NOT NULL,
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    [UpdateTime] DATETIME         DEFAULT (getdate()) NOT NULL,
    [GroupId]  UNIQUEIDENTIFIER   NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ProductToCategory_ToCategory] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[ContentPage] ([Id]),
    CONSTRAINT [FK_ProductToCategory_ToProduct] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([VersionId]),
	CONSTRAINT [FK_ProductToCategory_ToGroup] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[ContentPage] ([Id])
);

