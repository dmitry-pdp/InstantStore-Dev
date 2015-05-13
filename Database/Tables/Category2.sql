CREATE TABLE [dbo].[Category] (
    [VersionId]   UNIQUEIDENTIFIER NOT NULL,
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [Name]        NVARCHAR (250)   NOT NULL,
    [Image]       IMAGE            NULL,
    [ImageId]     UNIQUEIDENTIFIER NULL,
    [ListType]    INT              NOT NULL,
    [ShowPrices]  BIT              NOT NULL,
    [Description] NVARCHAR (MAX)   NULL,
    [Version]     INT              NOT NULL,
    [IsImportant] BIT NOT NULL DEFAULT 0, 
    PRIMARY KEY CLUSTERED ([VersionId] ASC)
);

