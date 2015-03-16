USE [InstantStore]
GO

/****** Object: Table [dbo].[User] Script Date: 3/15/2015 9:26:40 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[User] (
    [Id]                UNIQUEIDENTIFIER NOT NULL,
    [Name]              NVARCHAR (100)   NOT NULL,
    [Email]             NVARCHAR (100)   NULL,
    [Company]           NVARCHAR (250)   NULL,
    [Phonenumber]       NVARCHAR (50)    NULL,
    [City]              NVARCHAR (100)   NULL,
    [Password]          NVARCHAR (MAX)   NOT NULL,
    [IsAdmin]           BIT              NOT NULL,
    [IsActivated]       BIT              NOT NULL,
    [IsBlocked]         BIT              NOT NULL,
    [IsPaymentCash]     BIT              NOT NULL,
    [DefaultCurrencyId] UNIQUEIDENTIFIER NULL
);


