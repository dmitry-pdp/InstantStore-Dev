CREATE TABLE [dbo].[User] (
    [Id]                UNIQUEIDENTIFIER NOT NULL,
    [Name]              NVARCHAR (300)   NOT NULL,
    [Email]             NVARCHAR (100)   NULL,
    [Company]           NVARCHAR (250)   NULL,
    [Phonenumber]       NVARCHAR (50)    NULL,
    [City]              NVARCHAR (300)   NULL,
    [Password]          NVARCHAR (MAX)   NOT NULL,
    [IsAdmin]           BIT              NOT NULL,
    [IsActivated]       BIT              NOT NULL,
    [IsBlocked]         BIT              NOT NULL,
    [IsPaymentCash]     BIT              NOT NULL,
    [DefaultCurrencyId] UNIQUEIDENTIFIER NULL,
    [Comments] NVARCHAR(MAX) NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

