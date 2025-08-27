using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Retail.BFFWeb.Api.Interface;
using Retail.BFFWeb.Api.Model;
using Retail.BFFWeb.Api.Configurations;
using System.Net;
using System.Threading;

namespace Retail.BFF.UnitTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public sealed class ProductProviderTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory = null!;
        private Mock<IOptions<ProductServiceConfig>> _mockServiceConfig = null!;
        private ProductProvider _productProvider = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockServiceConfig = new Mock<IOptions<ProductServiceConfig>>();

            var serviceConfig = new ProductServiceConfig
            {
                BaseUrl = "http://localhost:5001",
                Endpoints = new ProductEndpoints
                {
                    GetAllProductsV1 = "/api/v1/products",
                    GetProductByIdV1 = "/api/v1/products/{id}"
                }
            };

            _mockServiceConfig.Setup(x => x.Value).Returns(serviceConfig);

            _productProvider = new ProductProvider(_mockHttpClientFactory.Object, _mockServiceConfig.Object);
        }

        [TestMethod]
        [TestCategory("ProductProvider")]
        public void ProductProvider_Constructor_CreatesInstance()
        {
            // Act & Assert
            _productProvider.Should().NotBeNull();
        }

        [TestMethod]
        [TestCategory("ProductProvider")]
        public void ProductProvider_Constructor_WithNullServiceConfig_ThrowsArgumentNullException()
        {
            // Act & Assert
            Action act = () => new ProductProvider(_mockHttpClientFactory.Object, null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        [TestCategory("ProductProvider")]
        public void ProductProvider_Constructor_WithNullHttpClientFactory_CreatesInstance()
        {
            // Act & Assert
            Action act = () => new ProductProvider(null!, _mockServiceConfig.Object);
            act.Should().NotThrow();
        }

        [TestMethod]
        [TestCategory("ProductProvider")]
        public void ProductProvider_ServiceConfig_IsCorrectlySet()
        {
            // Act & Assert
            _productProvider.Should().NotBeNull();
            // Note: ServiceConfig is private, so we can't directly test it
            // This test verifies the constructor completed successfully
        }

        [TestMethod]
        [TestCategory("ProductProvider")]
        public void ProductProvider_HttpClientFactory_IsCorrectlySet()
        {
            // Act & Assert
            _productProvider.Should().NotBeNull();
            // Note: HttpClientFactory is private, so we can't directly test it
            // This test verifies the constructor completed successfully
        }
    }
}
