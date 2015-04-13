CREATE TABLE [dbo].[Category]
(
    [VersionId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
	[Id] UNIQUEIDENTIFIER NOT NULL, 
    [Name] NVARCHAR(250) NOT NULL, 
    [ShowInMenu] BIT NOT NULL, 
    [Image] IMAGE NULL, 
	[ImageId] UNIQUEIDENTIFIER NULL,
    [ListType] INT NOT NULL, 
    [ShowPrices] BIT NOT NULL, 
    [Description] NVARCHAR(MAX) NULL, 
    [Version] INT NOT NULL, 
)
