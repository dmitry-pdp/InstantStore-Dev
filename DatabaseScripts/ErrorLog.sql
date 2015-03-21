﻿CREATE TABLE [dbo].[Table]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [ExceptionText] NVARCHAR(MAX) NOT NULL, 
    [DateTime] DATETIME NOT NULL, 
    [UserId] UNIQUEIDENTIFIER NULL, 
    [SessionId] NVARCHAR(MAX) NULL, 
    [RequestUrl] NVARCHAR(MAX) NOT NULL, 
    [ClientIP] NVARCHAR(50) NULL, 
    [UserAgent] NVARCHAR(250) NULL
)
