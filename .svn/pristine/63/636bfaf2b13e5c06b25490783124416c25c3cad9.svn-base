CREATE TABLE [dbo].[ProductCategories] (
    [ProductCategoryId] INT            IDENTITY (1, 1) NOT NULL,
    [Name]              NVARCHAR (100) COLLATE Cyrillic_General_CI_AI NOT NULL,
    [ParentCategory]    INT            NULL,
    [DisplayType]       INT            NULL,
    [IsVisibleInMenu]   BIT            DEFAULT ((0)) NOT NULL,
    [URLAlias]          NVARCHAR (100) NULL,
    [SortOrder]         INT            DEFAULT ((0)) NULL,
    PRIMARY KEY CLUSTERED ([ProductCategoryId] ASC)
);

