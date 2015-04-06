CREATE TABLE [dbo].[CustomProperty] (
    [Id]    UNIQUEIDENTIFIER NOT NULL,
    [Name]  NVARCHAR (250)   NOT NULL,
    [Value] NVARCHAR (MAX)   NULL,
    [TemplateId] UNIQUEIDENTIFIER NOT NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC), 
    CONSTRAINT [FK_CustomProperty_ToTablePropertyTemplateTable] FOREIGN KEY ([TemplateId]) REFERENCES [PropertyTemplate]([Id])
);

