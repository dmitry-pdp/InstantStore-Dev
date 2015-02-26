CREATE TABLE [dbo].[ProductsCategories] (
    [ProductId]         INT NOT NULL,
    [ProductCategoryID] INT NOT NULL,
    [SortOrder]         INT DEFAULT ((0)) NOT NULL,
    CONSTRAINT [FK_category_productsCategories] FOREIGN KEY ([ProductCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryId]) ON DELETE CASCADE,
    CONSTRAINT [FK_product_productsCategories] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductId]) ON DELETE CASCADE
);

