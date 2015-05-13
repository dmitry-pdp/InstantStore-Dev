CREATE TABLE [dbo].[OrderUpdates]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [OrderId] UNIQUEIDENTIFIER NOT NULL, 
    [DateTime] DATETIME NOT NULL, 
    [Status] INT NOT NULL, 
    CONSTRAINT [FK_OrderUpdates_ToTableOrders] FOREIGN KEY ([OrderId]) REFERENCES [Order]([Id])
)
