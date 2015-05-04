CREATE TABLE [dbo].[ContentPage] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [Name]           NVARCHAR (250)   NOT NULL,
    [Text]           NVARCHAR (MAX)   NULL,
    [ParentId]       UNIQUEIDENTIFIER NULL,
    [CategoryId]     UNIQUEIDENTIFIER NULL,
    [Position]       INT              NOT NULL,
    [AttachmentId]   UNIQUEIDENTIFIER NULL,
    [AttachmentName] NVARCHAR (250)   NULL,
    [ShowInMenu] BIT NOT NULL DEFAULT 1, 
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ContentPage_ToTableContentPage] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[ContentPage] ([Id]),
    CONSTRAINT [FK_ContentPage_ToTableCategory] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Category] ([VersionId]),
    CONSTRAINT [FK_ContentPage_ToTable] FOREIGN KEY ([AttachmentId]) REFERENCES [dbo].[Attachment] ([Id])
);

