CREATE TABLE [dbo].[Category]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [Name] NVARCHAR(250) NOT NULL, 
    [ParentCategoryId] UNIQUEIDENTIFIER NULL, 
    [Image] IMAGE NULL, 
    [ShowInMenu] BIT NOT NULL DEFAULT 0, 
    [ListType] TINYINT NULL, 
    [Text] TEXT NULL, 
    [Attachment] VARBINARY(MAX) NULL 
)
