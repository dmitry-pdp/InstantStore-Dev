using InstantStore.Domain.Abstract;
using InstantStore.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace InstantStore.WebUI.Tests.Controllers.NavCategoryController
{
    [TestClass]
    public class NavCategory
    {
        [TestMethod]
        public void Can_Create_Categories()
        {
            //Arrange
            Mock<IRepository> mock = new Mock<IRepository>();

            int pageSize = 5;
            int mockProductCount = pageSize + 4;
            Product[] data = new Product[mockProductCount];
            //add 1 different product
            data[0] = new Product
            {
                ProductID = 1,
                SortOrder = 1,
                Name = "P" + 1,
                Category = new ProductCategory()
                {
                    URLAlias = "Cat0",
                    Name = "Бананы"
                }
            };
            string categoryName;
            int categoryIndex;
            for (int i = 2; i <= mockProductCount; i++)
            {
                categoryIndex = Math.Round((Decimal)i % 2, MidpointRounding.AwayFromZero) == 0 ? 1 : 2;
                categoryName = categoryIndex == 1 ? "Апельсины" : "Яблоки";
                data[i - 1] = new Product
                {
                    ProductID = i,
                    SortOrder = i,
                    Name = "P" + i,
                    Category = new ProductCategory()
                    {
                        URLAlias = "Cat" + categoryIndex,
                        Name = categoryName
                    }
                };
            }

            mock.Setup(m => m.Products).Returns(data);

            //Arrange
            var target = new InstantStore.WebUI.Controllers.NavCategoryController(mock.Object);

            //Act
            ProductCategory[] results = (ProductCategory[])target.Menu().Model;

            //Assert
            Assert.AreEqual(results.Length, 3);
            Assert.AreEqual(results[0].Name, "Апельсины");
            Assert.AreEqual(results[1].Name, "Бананы");
            Assert.AreEqual(results[2].Name, "Яблоки");
        }

        [TestMethod]
        public void Indicates_Selected_Category()
        {
            //Arrange
            Mock<IRepository> mock = new Mock<IRepository>();
            Product[] data = new Product[] {
                new Product
                            {
                                ProductID = 1,
                                SortOrder = 1,
                                Name = "P" + 1,
                                Category = new ProductCategory()
                                {
                                    URLAlias = "Cat0",
                                    Name = "Бананы"
                                }
                            },
                new Product {
                                ProductID = 2,
                                SortOrder = 2,
                                Name = "P" + 2,
                                Category = new ProductCategory()
                                {
                                    URLAlias = "Cat1",
                                    Name = "Апельсины"
                                }
                }};           

            mock.Setup(m => m.Products).Returns(data);

            //Arrange
            var target = new InstantStore.WebUI.Controllers.NavCategoryController(mock.Object);
            string categoryToSelect = "Cat1";

            //Act
            string result = target.Menu(categoryToSelect).ViewBag.SelectedCategory;

            //Assert
            Assert.AreEqual(categoryToSelect, result);
        }


    }
}
