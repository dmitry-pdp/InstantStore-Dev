CREATE TABLE [dbo].[PropertyTemplate] (
    [Id]   UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR (250)   NOT NULL,
    [IsPrototype] BIT NOT NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

