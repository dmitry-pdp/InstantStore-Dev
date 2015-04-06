CREATE TABLE [dbo].[Settings] (
    [MainDescription] NVARCHAR (MAX) NULL,
    [HeaderHtml]      NVARCHAR (MAX) NULL,
    [FooterHtml]      NVARCHAR (MAX) NULL, 
    [Id] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [PK_Settings] PRIMARY KEY ([Id])
);

