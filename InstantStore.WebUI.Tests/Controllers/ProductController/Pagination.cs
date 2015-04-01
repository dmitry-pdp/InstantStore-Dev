using InstantStore.Domain.Abstract;
using InstantStore.Domain.Entities;
using InstantStore.WebUI.Controllers;
using InstantStore.WebUI.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.WebPages.Html;
using InstantStore.WebUI.HtmlHelpers;
using InstantStore.WebUI;

namespace InstantStore.WebUI.Tests.Controllers.ProductController
{
    [TestClass]
    public class Pagination
    { /*
        [TestMethod]
        public void Can_Paginate()
        {
            //Arrange
            Mock<IRepository> mock = new Mock<IRepository>();

            int pageSize = 5;
            int mockProductCount = pageSize + 4;
            Product[] data = new Product[mockProductCount];
            for (int i = 1; i<=mockProductCount; i++)
            {
                data[i-1] = new Product{ProductID = i, SortOrder = i, Name = "P"+i};
            }

            mock.Setup(m => m.Products).Returns(data);

            WebUI.Controllers.ProductController controller = new WebUI.Controllers.ProductController(mock.Object);
            controller.PageSize = pageSize;

            //Act
            var model = (ProductListViewModel)controller.List(null, 2).Model;

            //Assert
            Product[] prodArray = model.Products.ToArray();
            Assert.IsTrue(prodArray.Length == 4);
            Assert.AreEqual(prodArray[0].Name, "P6");
            Assert.AreEqual(prodArray[3].Name, "P9");
        }

        [TestMethod]
        public void Can_Generate_Page_Links()
        {
            //Arrange - define an Html Helper - we need to do this
            //in order to apply extension method
            System.Web.Mvc.HtmlHelper myHelper = null;

            //Arrange - create Paging info data
            PagingInfo pagingInfo = new PagingInfo
            {
                CurrentPage = 2,
                TotalItems = 28,
                ItemsPerPage = 10
            };

            //Arrange - set up a delegate using lambda expression
            Func<int, string> pageUrlDelegate = i => "Page" + i;

            //Act
            MvcHtmlString result = myHelper.PageLinks(pagingInfo, pageUrlDelegate);

            //Assert
            Assert.AreEqual(@"<a class=""btn btn-default"" href=""Page1"">1</a> "
                + @"<a class=""btn btn-default btn-primary selected"" href=""Page2"">2</a> "
                + @"<a class=""btn btn-default"" href=""Page3"">3</a>", result.ToString());
        }

        [TestMethod]
        public void Can_Generate_Page_Links_1_Page()
        {
            //Arrange - define an Html Helper - we need to do this
            //in order to apply extension method
            System.Web.Mvc.HtmlHelper myHelper = null;

            //Arrange - create Paging info data
            PagingInfo pagingInfo = new PagingInfo
            {
                CurrentPage = 1,
                TotalItems = 9,
                ItemsPerPage = 10
            };

            //Arrange - set up a delegate using lambda expression
            Func<int, string> pageUrlDelegate = i => "Page" + i;

            //Act
            MvcHtmlString result = myHelper.PageLinks(pagingInfo, pageUrlDelegate);

            //Assert
            Assert.AreEqual("", result.ToString());
        }

        [TestMethod]
        public void Can_Generate_Page_Links_No_Items()
        {
            //Arrange - define an Html Helper - we need to do this
            //in order to apply extension method
            System.Web.Mvc.HtmlHelper myHelper = null;

            //Arrange - create Paging info data
            PagingInfo pagingInfo = new PagingInfo
            {
                CurrentPage = 1,
                TotalItems = 0,
                ItemsPerPage = 10
            };

            //Arrange - set up a delegate using lambda expression
            Func<int, string> pageUrlDelegate = i => "Page" + i;

            //Act
            MvcHtmlString result = myHelper.PageLinks(pagingInfo, pageUrlDelegate);

            //Assert
            Assert.AreEqual("", result.ToString());
        }

        [TestMethod]
        public void Can_Send_Pagination_View_Model()
        {
            //Arrange
            Mock<IRepository> mock = new Mock<IRepository>();

            int pageSize = 5;
            int mockProductCount = pageSize + 4;
            Product[] data = new Product[mockProductCount];
            for (int i = 1; i <= mockProductCount; i++)
            {
                data[i - 1] = new Product { ProductID = i, SortOrder = i, Name = "P" + i };
            }

            mock.Setup(m => m.Products).Returns(data);

            //Arrange
            var controller = new InstantStore.WebUI.Controllers.ProductController(mock.Object);
            controller.PageSize = pageSize;

            //Act
            ProductListViewModel result = (ProductListViewModel)controller.List(null, 2).Model;

            //Assert
            PagingInfo pageInfo = result.PagingInfo;
            Assert.AreEqual(pageInfo.CurrentPage, 2);
            Assert.AreEqual(pageInfo.ItemsPerPage, pageSize);
            Assert.AreEqual(pageInfo.TotalItems, mockProductCount);
            Assert.AreEqual(pageInfo.TotalPages, 2);
        }

        [TestMethod]
        public void Can_Filter_Products()
        {
            //Arrange
            Mock<IRepository> mock = new Mock<IRepository>();

            int pageSize = 5;
            int mockProductCount = pageSize + 4;
            Product[] data = new Product[mockProductCount];
            for (int i = 1; i <= mockProductCount; i++)
            {
                data[i - 1] = new Product { ProductID = i, 
                                            SortOrder = i, 
                                            Name = "P" + i, 
                                            Category = new ProductCategory() {URLAlias = "Cat" + (Math.Round((Decimal)i%2,MidpointRounding.AwayFromZero) == 0 ? 1 : 2)} };
            }

            mock.Setup(m => m.Products).Returns(data);

            //Arrange
            var cont = new InstantStore.WebUI.Controllers.ProductController(mock.Object);
            cont.PageSize = pageSize;

            //Action
            Product[] result = ((ProductListViewModel)cont.List("Cat2", 1).Model).Products.ToArray();

            //Assert
            Assert.AreEqual(result.Length, 5);
            Assert.IsTrue(result[0].Name == "P1" && result[0].Category.URLAlias == "Cat2");
            Assert.IsTrue(result[1].Name == "P3" && result[1].Category.URLAlias == "Cat2");
        }
       */
    }
}
