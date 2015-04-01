CREATE TABLE [dbo].[Image] (
    [Id]    UNIQUEIDENTIFIER NOT NULL,
    [Image] IMAGE            NOT NULL,
    [ProductId] UNIQUEIDENTIFIER NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC), 
    CONSTRAINT [FK_Image_ToTableProduct] FOREIGN KEY ([ProductId]) REFERENCES [Product]([Id])
);

