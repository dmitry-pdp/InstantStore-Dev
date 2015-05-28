CREATE TABLE [dbo].[Settings] (
    [Key] NVARCHAR (250)   NOT NULL,
    [Value]      NVARCHAR (MAX)   NOT NULL,
    CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED ([Key])
);

