CREATE TABLE [dbo].[Attachment]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [Name] NVARCHAR(250) NOT NULL, 
    [Content] VARBINARY(MAX) NOT NULL, 
    [ContentType] NVARCHAR(250) NOT NULL, 
    [ContentLength] INT NOT NULL, 
    [UploadedAt] DATETIME NOT NULL 
)
