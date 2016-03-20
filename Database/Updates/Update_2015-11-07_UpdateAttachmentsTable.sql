CREATE TABLE [dbo].[ContentPageAttachment]
(
	[AttachmentId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [PageId] UNIQUEIDENTIFIER NOT NULL, 
    [Name] NVARCHAR(250) NOT NULL, 
    [ContentType] NVARCHAR(250) NOT NULL, 
    [Size] INT NOT NULL, 
	[Order] INT NOT NULL,
    CONSTRAINT [FK_ContentPageAttachment_ToContentPage] FOREIGN KEY ([PageId]) REFERENCES [ContentPage]([Id]), 
    CONSTRAINT [FK_ContentPageAttachment_ToAttachment] FOREIGN KEY ([AttachmentId]) REFERENCES [Attachment]([Id])
);

INSERT INTO [ContentPageAttachment]
(AttachmentId, PageId, Name, ContentType, Size, [Order])
SELECT Attachment.Id, ContentPage.Id, Attachment.Name, Attachment.ContentType, Attachment.ContentLength, 1
FROM Attachment, ContentPage
WHERE Attachment.Id = ContentPage.AttachmentId;

ALTER TABLE ContentPage DROP CONSTRAINT FK_ContentPage_ToTable;
ALTER TABLE ContentPage DROP COLUMN [AttachmentId];
