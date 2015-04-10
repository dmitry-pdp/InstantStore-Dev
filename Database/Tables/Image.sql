CREATE TABLE [dbo].[Image] (
    [Id]    UNIQUEIDENTIFIER NOT NULL,
    [Image] IMAGE            NOT NULL,
	[ImageContentType] NVARCHAR(250) NOT NULL,
    [ProductId] UNIQUEIDENTIFIER NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

