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
    PRIMARY KEY CLUSTERED ([VersionId] ASC)
);
