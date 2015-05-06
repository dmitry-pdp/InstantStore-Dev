CREATE TABLE [dbo].[ImageThumbnails]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [LargeThumbnail] IMAGE NOT NULL, 
    [SmallThumbnail] IMAGE NOT NULL, 
    CONSTRAINT [FK_Table_ToTableImages] FOREIGN KEY ([Id]) REFERENCES [Image]([Id])
)
