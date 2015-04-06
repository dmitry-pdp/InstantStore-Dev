﻿/*
Deployment script for InstantStore

This code was generated by a tool.
Changes to this file may cause incorrect behavior and will be lost if
the code is regenerated.
*/

GO
SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;


GO
:setvar DatabaseName "InstantStore"
:setvar DefaultFilePrefix "InstantStore"
:setvar DefaultDataPath "C:\Users\dzpotash\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\v11.0\"
:setvar DefaultLogPath "C:\Users\dzpotash\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\v11.0\"

GO
:on error exit
GO
/*
Detect SQLCMD mode and disable script execution if SQLCMD mode is not supported.
To re-enable the script after enabling SQLCMD mode, execute the following:
SET NOEXEC OFF; 
*/
:setvar __IsSqlCmdEnabled "True"
GO
IF N'$(__IsSqlCmdEnabled)' NOT LIKE N'True'
    BEGIN
        PRINT N'SQLCMD mode must be enabled to successfully execute this script.';
        SET NOEXEC ON;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'$(DatabaseName)')
    BEGIN
        ALTER DATABASE [$(DatabaseName)]
            SET ANSI_NULLS ON,
                ANSI_PADDING ON,
                ANSI_WARNINGS ON,
                ARITHABORT ON,
                CONCAT_NULL_YIELDS_NULL ON,
                QUOTED_IDENTIFIER ON,
                ANSI_NULL_DEFAULT ON,
                CURSOR_DEFAULT LOCAL 
            WITH ROLLBACK IMMEDIATE;
        ALTER DATABASE [$(DatabaseName)]
            SET AUTO_CLOSE OFF 
            WITH ROLLBACK IMMEDIATE;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'$(DatabaseName)')
    BEGIN
        ALTER DATABASE [$(DatabaseName)]
            SET PAGE_VERIFY NONE,
                DISABLE_BROKER 
            WITH ROLLBACK IMMEDIATE;
    END


GO
USE [$(DatabaseName)];


GO
PRINT N'Rename refactoring operation with key 5cf1951d-83b7-4904-ac05-126fb77309c5 is skipped, element [dbo].[Products].[Size] (SqlSimpleColumn) will not be renamed to ProductSizeID';


GO
PRINT N'Creating [dbo].[ProductCategories]...';


GO
CREATE TABLE [dbo].[ProductCategories] (
    [ProductCategoryId] INT            IDENTITY (1, 1) NOT NULL,
    [Name]              NVARCHAR (100) COLLATE Cyrillic_General_CI_AI NOT NULL,
    [ParentCategory]    INT            NULL,
    [DisplayType]       INT            NULL,
    [IsVisibleInMenu]   BIT            NOT NULL,
    [URLAlias]          NVARCHAR (100) NULL,
    [SortOrder]         INT            NULL,
    PRIMARY KEY CLUSTERED ([ProductCategoryId] ASC)
);


GO
PRINT N'Creating [dbo].[Products]...';


GO
CREATE TABLE [dbo].[Products] (
    [ProductId]     INT             IDENTITY (1, 1) NOT NULL,
    [Name]          NVARCHAR (100)  COLLATE Cyrillic_General_CI_AI NOT NULL,
    [Description]   NTEXT           COLLATE Cyrillic_General_CI_AI NOT NULL,
    [PriceCash]     DECIMAL (16, 2) NOT NULL,
    [PriceCashless] DECIMAL (16, 2) NOT NULL,
    [ProductSizeID] INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([ProductId] ASC)
);


GO
PRINT N'Creating [dbo].[ProductsCategories]...';


GO
CREATE TABLE [dbo].[ProductsCategories] (
    [ProductId]         INT NOT NULL,
    [ProductCategoryID] INT NOT NULL,
    [SortOrder]         INT NOT NULL
);


GO
PRINT N'Creating [dbo].[ProductSize]...';


GO
CREATE TABLE [dbo].[ProductSize] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [Name]      NVARCHAR (50) NOT NULL,
    [SortOrder] INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
PRINT N'Creating [dbo].[User]...';


GO
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
    [Comments]          NVARCHAR (MAX)   NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
PRINT N'Creating Default Constraint on [dbo].[ProductCategories]....';


GO
ALTER TABLE [dbo].[ProductCategories]
    ADD DEFAULT ((0)) FOR [IsVisibleInMenu];


GO
PRINT N'Creating Default Constraint on [dbo].[ProductCategories]....';


GO
ALTER TABLE [dbo].[ProductCategories]
    ADD DEFAULT ((0)) FOR [SortOrder];


GO
PRINT N'Creating Default Constraint on [dbo].[Products]....';


GO
ALTER TABLE [dbo].[Products]
    ADD DEFAULT ('') FOR [Description];


GO
PRINT N'Creating Default Constraint on [dbo].[Products]....';


GO
ALTER TABLE [dbo].[Products]
    ADD DEFAULT ((0)) FOR [PriceCash];


GO
PRINT N'Creating Default Constraint on [dbo].[Products]....';


GO
ALTER TABLE [dbo].[Products]
    ADD DEFAULT ((0)) FOR [PriceCashless];


GO
PRINT N'Creating Default Constraint on [dbo].[Products]....';


GO
ALTER TABLE [dbo].[Products]
    ADD DEFAULT ((1)) FOR [ProductSizeID];


GO
PRINT N'Creating Default Constraint on [dbo].[ProductsCategories]....';


GO
ALTER TABLE [dbo].[ProductsCategories]
    ADD DEFAULT ((0)) FOR [SortOrder];


GO
PRINT N'Creating Default Constraint on [dbo].[ProductSize]....';


GO
ALTER TABLE [dbo].[ProductSize]
    ADD DEFAULT ((0)) FOR [SortOrder];


GO
PRINT N'Creating FK_productSize...';


GO
ALTER TABLE [dbo].[Products] WITH NOCHECK
    ADD CONSTRAINT [FK_productSize] FOREIGN KEY ([ProductSizeID]) REFERENCES [dbo].[ProductSize] ([Id]) ON DELETE CASCADE;


GO
PRINT N'Creating FK_category_productsCategories...';


GO
ALTER TABLE [dbo].[ProductsCategories] WITH NOCHECK
    ADD CONSTRAINT [FK_category_productsCategories] FOREIGN KEY ([ProductCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryId]) ON DELETE CASCADE;


GO
PRINT N'Creating FK_product_productsCategories...';


GO
ALTER TABLE [dbo].[ProductsCategories] WITH NOCHECK
    ADD CONSTRAINT [FK_product_productsCategories] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductId]) ON DELETE CASCADE;


GO
-- Refactoring step to update target server with deployed transaction logs

IF OBJECT_ID(N'dbo.__RefactorLog') IS NULL
BEGIN
    CREATE TABLE [dbo].[__RefactorLog] (OperationKey UNIQUEIDENTIFIER NOT NULL PRIMARY KEY)
    EXEC sp_addextendedproperty N'microsoft_database_tools_support', N'refactoring log', N'schema', N'dbo', N'table', N'__RefactorLog'
END
GO
IF NOT EXISTS (SELECT OperationKey FROM [dbo].[__RefactorLog] WHERE OperationKey = '5cf1951d-83b7-4904-ac05-126fb77309c5')
INSERT INTO [dbo].[__RefactorLog] (OperationKey) values ('5cf1951d-83b7-4904-ac05-126fb77309c5')

GO

GO
SET IDENTITY_INSERT [dbo].[ProductCategories] ON
INSERT INTO [dbo].[ProductCategories] ([ProductCategoryId], [Name], [ParentCategory], [DisplayType], [IsVisibleInMenu], [URLAlias], [SortOrder]) VALUES (1, N'Завальцовка', 3, 1, 1, N'Zavaltsovka', 1)
INSERT INTO [dbo].[ProductCategories] ([ProductCategoryId], [Name], [ParentCategory], [DisplayType], [IsVisibleInMenu], [URLAlias], [SortOrder]) VALUES (2, N'Шарики', 3, 1, 1, N'Shariki', 2)
INSERT INTO [dbo].[ProductCategories] ([ProductCategoryId], [Name], [ParentCategory], [DisplayType], [IsVisibleInMenu], [URLAlias], [SortOrder]) VALUES (3, N'Серьги для прокалывания мочки уха', NULL, 1, 1, N'EarPiercing', 1)
SET IDENTITY_INSERT [dbo].[ProductCategories] OFF

SET IDENTITY_INSERT [dbo].[ProductSize] ON
INSERT INTO [dbo].[ProductSize] ([Id], [Name], [SortOrder]) VALUES (1, N'Миди', 0)
INSERT INTO [dbo].[ProductSize] ([Id], [Name], [SortOrder]) VALUES (2, N'Мини', 1)
INSERT INTO [dbo].[ProductSize] ([Id], [Name], [SortOrder]) VALUES (3, N'Макси', 2)
SET IDENTITY_INSERT [dbo].[ProductSize] OFF

SET IDENTITY_INSERT [dbo].[Products] ON
INSERT INTO [dbo].[Products] ([ProductId], [Name], [Description], [PriceCash], [PriceCashless], [ProductSizeID]) VALUES (1, N'Шарик', N'Золотой шарик среднего размера', CAST(5.00 AS Decimal(16, 2)), CAST(15000.00 AS Decimal(16, 2)), 1)
INSERT INTO [dbo].[Products] ([ProductId], [Name], [Description], [PriceCash], [PriceCashless], [ProductSizeID]) VALUES (2, N'Звездочка', N'Золотая звездочка среднего размера', CAST(5.00 AS Decimal(16, 2)), CAST(15000.00 AS Decimal(16, 2)), 1)
INSERT INTO [dbo].[Products] ([ProductId], [Name], [Description], [PriceCash], [PriceCashless], [ProductSizeID]) VALUES (3, N'Звездочка', N'Золотая звездочка маленького размера', CAST(5.50 AS Decimal(16, 2)), CAST(17000.00 AS Decimal(16, 2)), 2)
INSERT INTO [dbo].[Products] ([ProductId], [Name], [Description], [PriceCash], [PriceCashless], [ProductSizeID]) VALUES (4, N'Сапфир', N'Сапфир в золотой завальцовке среднего размера', CAST(5.00 AS Decimal(16, 2)), CAST(15000.00 AS Decimal(16, 2)), 1)
INSERT INTO [dbo].[Products] ([ProductId], [Name], [Description], [PriceCash], [PriceCashless], [ProductSizeID]) VALUES (5, N'Хрусталь', N'Хрусталь в золотой завальцовке среднего размера', CAST(5.00 AS Decimal(16, 2)), CAST(15000.00 AS Decimal(16, 2)), 1)
INSERT INTO [dbo].[Products] ([ProductId], [Name], [Description], [PriceCash], [PriceCashless], [ProductSizeID]) VALUES (6, N'Хрусталь', N'Хрусталь в золотой завальцовке маленького размера', CAST(5.50 AS Decimal(16, 2)), CAST(17000.00 AS Decimal(16, 2)), 2)
SET IDENTITY_INSERT [dbo].[Products] OFF

INSERT INTO [dbo].[ProductsCategories] ([ProductId], [ProductCategoryID], [SortOrder]) VALUES (1, 2, 1)
INSERT INTO [dbo].[ProductsCategories] ([ProductId], [ProductCategoryID], [SortOrder]) VALUES (2, 2, 2)
INSERT INTO [dbo].[ProductsCategories] ([ProductId], [ProductCategoryID], [SortOrder]) VALUES (3, 2, 1)
INSERT INTO [dbo].[ProductsCategories] ([ProductId], [ProductCategoryID], [SortOrder]) VALUES (4, 1, 1)
INSERT INTO [dbo].[ProductsCategories] ([ProductId], [ProductCategoryID], [SortOrder]) VALUES (5, 1, 2)
INSERT INTO [dbo].[ProductsCategories] ([ProductId], [ProductCategoryID], [SortOrder]) VALUES (5, 1, 3)
GO

GO
PRINT N'Checking existing data against newly created constraints';


GO
USE [$(DatabaseName)];


GO
ALTER TABLE [dbo].[Products] WITH CHECK CHECK CONSTRAINT [FK_productSize];

ALTER TABLE [dbo].[ProductsCategories] WITH CHECK CHECK CONSTRAINT [FK_category_productsCategories];

ALTER TABLE [dbo].[ProductsCategories] WITH CHECK CHECK CONSTRAINT [FK_product_productsCategories];


GO
PRINT N'Update complete.';


GO
