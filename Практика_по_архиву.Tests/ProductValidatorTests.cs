using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Практика_по_архиву;
using Практика_по_архиву.DB_;
using System.Linq;
using System.Data.Entity;

namespace Практика_по_архиву.Tests
{
    [TestClass]
    public class ProductValidatorTests
    {
        private Mock<ПрактикаEntities> _dbContextMock;
        private ProductValidator _validator;

        [TestInitialize]
        public void Setup()
        {
            _dbContextMock = new Mock<ПрактикаEntities>();
            _validator = new ProductValidator(_dbContextMock.Object);
        }

        // Низкая сложность (10 тестов)

        [TestMethod]
        public void ValidateProductData_EmptyArticleNumber_ReturnsFalse()
        {
            var result = _validator.ValidateProductData("", "Product Title", new ProductType(), "5", "10", "100");
            Assert.IsFalse(result.IsValid, "Метод должен вернуть false, если артикул пустой.");
            Assert.AreEqual("Артикул не может быть пустым.", result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateProductData_EmptyTitle_ReturnsFalse()
        {
            var result = _validator.ValidateProductData("ART123", "", new ProductType(), "5", "10", "100");
            Assert.IsFalse(result.IsValid, "Метод должен вернуть false, если наименование пустое.");
            Assert.AreEqual("Наименование не может быть пустым.", result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateProductData_NullProductType_ReturnsFalse()
        {
            var result = _validator.ValidateProductData("ART123", "Product Title", null, "5", "10", "100");
            Assert.IsFalse(result.IsValid, "Метод должен вернуть false, если тип продукта не выбран.");
            Assert.AreEqual("Тип продукта должен быть выбран.", result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateProductData_NegativePersonCount_ReturnsFalse()
        {
            var result = _validator.ValidateProductData("ART123", "Product Title", new ProductType(), "-5", "10", "100");
            Assert.IsFalse(result.IsValid, "Метод должен вернуть false, если количество человек отрицательное.");
            Assert.AreEqual("Количество человек для производства должно быть неотрицательным числом.", result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateProductData_InvalidPersonCount_ReturnsFalse()
        {
            var result = _validator.ValidateProductData("ART123", "Product Title", new ProductType(), "invalid", "10", "100");
            Assert.IsFalse(result.IsValid, "Метод должен вернуть false, если количество человек не является числом.");
            Assert.AreEqual("Количество человек для производства должно быть неотрицательным числом.", result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateProductData_NegativeWorkshopNumber_ReturnsFalse()
        {
            var result = _validator.ValidateProductData("ART123", "Product Title", new ProductType(), "5", "-10", "100");
            Assert.IsFalse(result.IsValid, "Метод должен вернуть false, если номер цеха отрицательный.");
            Assert.AreEqual("Номер цеха должен быть неотрицательным числом.", result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateProductData_InvalidWorkshopNumber_ReturnsFalse()
        {
            var result = _validator.ValidateProductData("ART123", "Product Title", new ProductType(), "5", "invalid", "100");
            Assert.IsFalse(result.IsValid, "Метод должен вернуть false, если номер цеха не является числом.");
            Assert.AreEqual("Номер цеха должен быть неотрицательным числом.", result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateProductData_NegativeMinCost_ReturnsFalse()
        {
            var result = _validator.ValidateProductData("ART123", "Product Title", new ProductType(), "5", "10", "-100");
            Assert.IsFalse(result.IsValid, "Метод должен вернуть false, если минимальная стоимость отрицательная.");
            Assert.AreEqual("Минимальная стоимость для агента должна быть неотрицательной.", result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateProductData_InvalidMinCost_ReturnsFalse()
        {
            var result = _validator.ValidateProductData("ART123", "Product Title", new ProductType(), "5", "10", "invalid");
            Assert.IsFalse(result.IsValid, "Метод должен вернуть false, если минимальная стоимость не является числом.");
            Assert.AreEqual("Минимальная стоимость для агента должна быть неотрицательной.", result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateProductData_ValidData_ReturnsTrue()
        {
            var result = _validator.ValidateProductData("ART123", "Product Title", new ProductType(), "5", "10", "100");
            Assert.IsTrue(result.IsValid, "Метод должен вернуть true для корректных данных.");
            Assert.AreEqual(string.Empty, result.ErrorMessage);
        }

        // Высокая сложность (5 тестов)

        [TestMethod]
        public void CheckArticleNumberUniqueness_ExistingArticleNumberWhenAdding_ReturnsFalse()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ID = 1, ArticleNumber = "ART123" }
            }.AsQueryable();
            var productSet = new Mock<DbSet<Product>>();
            productSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.Provider);
            productSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.Expression);
            productSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.ElementType);
            productSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.GetEnumerator());
            _dbContextMock.Setup(db => db.Product).Returns(productSet.Object);

            // Act
            var result = _validator.CheckArticleNumberUniqueness("ART123", false, null);

            // Assert
            Assert.IsFalse(result.IsUnique, "Метод должен вернуть false, если артикул уже существует при добавлении.");
            Assert.AreEqual("Продукт с таким артикулом уже существует.", result.ErrorMessage);
        }

        [TestMethod]
        public void CheckArticleNumberUniqueness_ExistingArticleNumberWhenEditing_ReturnsFalse()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ID = 1, ArticleNumber = "ART123" },
                new Product { ID = 2, ArticleNumber = "ART456" }
            }.AsQueryable();
            var productSet = new Mock<DbSet<Product>>();
            productSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.Provider);
            productSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.Expression);
            productSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.ElementType);
            productSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.GetEnumerator());
            _dbContextMock.Setup(db => db.Product).Returns(productSet.Object);
            var product = new Product { ID = 2, ArticleNumber = "ART456" };

            // Act
            var result = _validator.CheckArticleNumberUniqueness("ART123", true, product);

            // Assert
            Assert.IsFalse(result.IsUnique, "Метод должен вернуть false, если артикул уже существует у другого продукта при редактировании.");
            Assert.AreEqual("Продукт с таким артикулом уже существует.", result.ErrorMessage);
        }

        [TestMethod]
        public void CheckArticleNumberUniqueness_SameArticleNumberWhenEditing_ReturnsTrue()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ID = 1, ArticleNumber = "ART123" }
            }.AsQueryable();
            var productSet = new Mock<DbSet<Product>>();
            productSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.Provider);
            productSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.Expression);
            productSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.ElementType);
            productSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.GetEnumerator());
            _dbContextMock.Setup(db => db.Product).Returns(productSet.Object);
            var product = new Product { ID = 1, ArticleNumber = "ART123" };

            // Act
            var result = _validator.CheckArticleNumberUniqueness("ART123", true, product);

            // Assert
            Assert.IsTrue(result.IsUnique, "Метод должен вернуть true, если артикул не изменился при редактировании.");
            Assert.AreEqual(string.Empty, result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateMaterialAddition_NullMaterial_ReturnsFalse()
        {
            var result = _validator.ValidateMaterialAddition(null, "5");
            Assert.IsFalse(result.IsValid, "Метод должен вернуть false, если материал не выбран.");
            Assert.AreEqual("Материал должен быть выбран.", result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateMaterialAddition_NegativeCount_ReturnsFalse()
        {
            var material = new Material { ID = 1, Title = "Material 1" };
            var result = _validator.ValidateMaterialAddition(material, "-5");
            Assert.IsFalse(result.IsValid, "Метод должен вернуть false, если количество материала отрицательное.");
            Assert.AreEqual("Количество материала должно быть положительным числом.", result.ErrorMessage);
        }
    }
}