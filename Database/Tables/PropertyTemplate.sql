CREATE TABLE [dbo].[PropertyTemplate] (
    [Id]   UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR (250)   NOT NULL,
    [IsPrototype] BIT NOT NULL, 
	[PrototypeId] UNIQUEIDENTIFIER NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

