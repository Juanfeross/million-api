using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using API.Controllers;
using API.Models;
using FluentAssertions;
using Moq;

namespace MillionBack.Tests.Controllers;

[TestFixture]
public class PropertiesControllerTests
{
    private Mock<IPropertyService> _propertyServiceMock = null!;
    private Mock<ILogger<PropertiesController>> _loggerMock = null!;
    private PropertiesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _propertyServiceMock = new Mock<IPropertyService>();
        _loggerMock = new Mock<ILogger<PropertiesController>>();
        _controller = new PropertiesController(_propertyServiceMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task GetAllProperties_WhenCalled_ReturnsOkWithPagedResult()
    {
        // Arrange
        var page = 1;
        var pageSize = 20;
        var pagedResult = new PagedResult<PropertyDto>
        {
            Items = new List<PropertyDto>
            {
                new PropertyDto { IdProperty = "PROP001", Name = "Casa 1", Price = 100000 },
                new PropertyDto { IdProperty = "PROP002", Name = "Casa 2", Price = 200000 }
            },
            Total = 2,
            Page = page,
            PageSize = pageSize
        };

        _propertyServiceMock
            .Setup(x => x.GetAllPropertiesPagedAsync(page, pageSize))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAllProperties(page, pageSize);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);

        var response = okResult.Value as ApiResponse<PagedResult<PropertyDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(2);
        response.Data.Total.Should().Be(2);
    }

    [Test]
    public async Task GetAllProperties_WhenPageIsZero_UsesDefaultPage()
    {
        // Arrange
        var pagedResult = new PagedResult<PropertyDto>
        {
            Items = new List<PropertyDto>(),
            Total = 0,
            Page = 1,
            PageSize = 20
        };

        _propertyServiceMock
            .Setup(x => x.GetAllPropertiesPagedAsync(1, 20))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAllProperties(0, 0);

        // Assert
        _propertyServiceMock.Verify(x => x.GetAllPropertiesPagedAsync(1, 20), Times.Once);
    }

    [Test]
    public async Task GetAllProperties_WhenPageSizeExceedsMax_LimitsToMaxPageSize()
    {
        // Arrange
        var pagedResult = new PagedResult<PropertyDto>
        {
            Items = new List<PropertyDto>(),
            Total = 0,
            Page = 1,
            PageSize = 100
        };

        _propertyServiceMock
            .Setup(x => x.GetAllPropertiesPagedAsync(1, 100))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAllProperties(1, 200);

        // Assert
        _propertyServiceMock.Verify(x => x.GetAllPropertiesPagedAsync(1, 100), Times.Once);
    }

    [Test]
    public async Task GetAllProperties_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _propertyServiceMock
            .Setup(x => x.GetAllPropertiesPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllProperties(1, 20);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);

        var response = objectResult.Value as ApiResponse<object>;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Contain("Error al obtener las propiedades");
    }

    [Test]
    public async Task GetPropertyById_WhenPropertyExists_ReturnsOkWithPropertyDetail()
    {
        // Arrange
        var propertyId = "507f1f77bcf86cd799439011";
        var propertyDetail = new PropertyDetailDto
        {
            IdProperty = "PROP001",
            Name = "Casa de Prueba",
            Address = "Calle Test 123",
            Price = 250000
        };

        _propertyServiceMock
            .Setup(x => x.GetPropertyByIdAsync(propertyId))
            .ReturnsAsync(propertyDetail);

        // Act
        var result = await _controller.GetPropertyById(propertyId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);

        var response = okResult.Value as ApiResponse<PropertyDetailDto>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data!.Name.Should().Be("Casa de Prueba");
    }

    [Test]
    public async Task GetPropertyById_WhenPropertyDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var propertyId = "507f1f77bcf86cd799439011";

        _propertyServiceMock
            .Setup(x => x.GetPropertyByIdAsync(propertyId))
            .ReturnsAsync((PropertyDetailDto?)null);

        // Act
        var result = await _controller.GetPropertyById(propertyId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);

        var response = notFoundResult.Value as ApiResponse<object>;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Propiedad no encontrada");
    }

    [Test]
    public async Task GetPropertyById_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var propertyId = "507f1f77bcf86cd799439011";

        _propertyServiceMock
            .Setup(x => x.GetPropertyByIdAsync(propertyId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetPropertyById(propertyId);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Test]
    public async Task SearchProperties_WhenCalledWithFilters_ReturnsOkWithFilteredResults()
    {
        // Arrange
        var filter = new PropertyFilterDto
        {
            Name = "Casa",
            MinPrice = 100000,
            MaxPrice = 300000
        };
        var page = 1;
        var pageSize = 20;
        var pagedResult = new PagedResult<PropertyDto>
        {
            Items = new List<PropertyDto>
            {
                new PropertyDto { IdProperty = "PROP001", Name = "Casa de Prueba", Price = 200000 }
            },
            Total = 1,
            Page = page,
            PageSize = pageSize
        };

        _propertyServiceMock
            .Setup(x => x.SearchPropertiesPagedAsync(
                It.Is<PropertyFilterDto>(f => f.Name == "Casa" && f.MinPrice == 100000 && f.MaxPrice == 300000),
                page,
                pageSize))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.SearchProperties("Casa", null, 100000, 300000, page, pageSize);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);

        var response = okResult.Value as ApiResponse<PagedResult<PropertyDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Items.Should().HaveCount(1);
    }

    [Test]
    public async Task SearchProperties_WhenNoFiltersProvided_ReturnsOkWithAllResults()
    {
        // Arrange
        var pagedResult = new PagedResult<PropertyDto>
        {
            Items = new List<PropertyDto>(),
            Total = 0,
            Page = 1,
            PageSize = 20
        };

        _propertyServiceMock
            .Setup(x => x.SearchPropertiesPagedAsync(It.IsAny<PropertyFilterDto>(), 1, 20))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.SearchProperties(null, null, null, null, 1, 20);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _propertyServiceMock.Verify(
            x => x.SearchPropertiesPagedAsync(
                It.Is<PropertyFilterDto>(f => f.Name == null && f.Address == null && f.MinPrice == null && f.MaxPrice == null),
                1,
                20),
            Times.Once);
    }

    [Test]
    public async Task SearchProperties_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _propertyServiceMock
            .Setup(x => x.SearchPropertiesPagedAsync(It.IsAny<PropertyFilterDto>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.SearchProperties("Casa", null, null, null, 1, 20);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}

