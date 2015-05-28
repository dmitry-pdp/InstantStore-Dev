CREATE TABLE [dbo].[Feedback] (
    [Id]      UNIQUEIDENTIFIER NOT NULL,
    [Name]    NVARCHAR (50)    NOT NULL,
    [Email]   NVARCHAR (50)    NOT NULL,
    [Message] NVARCHAR (MAX)   NOT NULL,
    [Submitted] DATETIME NOT NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

