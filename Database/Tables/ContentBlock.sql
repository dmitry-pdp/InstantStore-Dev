CREATE TABLE [dbo].[ContentPage]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [Name] NVARCHAR(250) NOT NULL, 
    [Text] NVARCHAR(MAX) NULL, 
    [Attachment] VARBINARY(MAX) NULL, 
    [ContentType] INT NOT NULL, 
    [ParentId] UNIQUEIDENTIFIER NULL, 
    [ProductId] UNIQUEIDENTIFIER NULL, 
    [CategoryId] UNIQUEIDENTIFIER NULL, 
    [Position] INT NOT NULL, 
    [AttachmentType] NVARCHAR(150) NULL, 
    CONSTRAINT [FK_ContentPage_ToTableContentPage] FOREIGN KEY ([ParentId]) REFERENCES [ContentPage]([Id]), 
    CONSTRAINT [FK_ContentPage_ToTableProduct] FOREIGN KEY ([ProductId]) REFERENCES [Product]([VersionId]), 
    CONSTRAINT [FK_ContentPage_ToTableCategory] FOREIGN KEY ([CategoryId]) REFERENCES [Category]([VersionId])
)
