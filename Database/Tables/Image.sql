CREATE TABLE [dbo].[Image] (
    [Id]    UNIQUEIDENTIFIER NOT NULL,
    [Image] IMAGE            NOT NULL,
    [ProductId] UNIQUEIDENTIFIER NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

