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
