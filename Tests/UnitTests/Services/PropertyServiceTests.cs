using AutoMapper;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace MillionBack.Tests.Services;

[TestFixture]
public class PropertyServiceTests
{
    private Mock<IPropertyRepository> _propertyRepositoryMock = null!;
    private Mock<IOwnerRepository> _ownerRepositoryMock = null!;
    private Mock<IPropertyImageRepository> _propertyImageRepositoryMock = null!;
    private Mock<IPropertyTraceRepository> _propertyTraceRepositoryMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private IMemoryCache _memoryCache = null!;
    private PropertyService _propertyService = null!;

    [SetUp]
    public void SetUp()
    {
        _propertyRepositoryMock = new Mock<IPropertyRepository>();
        _ownerRepositoryMock = new Mock<IOwnerRepository>();
        _propertyImageRepositoryMock = new Mock<IPropertyImageRepository>();
        _propertyTraceRepositoryMock = new Mock<IPropertyTraceRepository>();
        _mapperMock = new Mock<IMapper>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _propertyService = new PropertyService(
            _propertyRepositoryMock.Object,
            _ownerRepositoryMock.Object,
            _propertyImageRepositoryMock.Object,
            _propertyTraceRepositoryMock.Object,
            _mapperMock.Object,
            _memoryCache
        );
    }

    [TearDown]
    public void TearDown()
    {
        _memoryCache.Dispose();
    }

    [Test]
    public async Task GetPropertyByIdAsync_WhenPropertyExists_ReturnsPropertyDetailDto()
    {
        // Arrange
        var propertyId = "507f1f77bcf86cd799439011";
        var property = new Property
        {
            Id = propertyId,
            IdProperty = "PROP001",
            Name = "Casa de Prueba",
            Address = "Calle Test 123",
            Price = 250000,
            IdOwner = "OWNER001"
        };

        var propertyDetailDto = new PropertyDetailDto
        {
            IdProperty = property.IdProperty,
            Name = property.Name,
            Address = property.Address,
            Price = property.Price
        };

        _propertyRepositoryMock
            .Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        _mapperMock
            .Setup(x => x.Map<PropertyDetailDto>(property))
            .Returns(propertyDetailDto);

        _ownerRepositoryMock
            .Setup(x => x.GetOwnersByIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, Owner>());

        _propertyImageRepositoryMock
            .Setup(x => x.GetEnabledImagesByPropertyIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, List<PropertyImage>>());

        _propertyTraceRepositoryMock
            .Setup(x => x.GetTracesByPropertyIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, List<PropertyTrace>>());

        // Act
        var result = await _propertyService.GetPropertyByIdAsync(propertyId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Casa de Prueba");
        result.Address.Should().Be("Calle Test 123");
        result.Price.Should().Be(250000);

        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
    }

    [Test]
    public async Task GetPropertyByIdAsync_WhenPropertyDoesNotExist_ReturnsNull()
    {
        // Arrange
        var propertyId = "507f1f77bcf86cd799439011";

        _propertyRepositoryMock
            .Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync((Property?)null);

        // Act
        var result = await _propertyService.GetPropertyByIdAsync(propertyId);

        // Assert
        result.Should().BeNull();
        _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
    }

    [Test]
    public async Task GetAllPropertiesPagedAsync_WhenCalled_ReturnsPagedResult()
    {
        // Arrange
        var page = 1;
        var pageSize = 20;
        var properties = new List<Property>
        {
            new Property
            {
                Id = "1",
                IdProperty = "PROP001",
                Name = "Casa 1",
                Address = "Calle 1",
                Price = 100000,
                IdOwner = "OWNER001"
            },
            new Property
            {
                Id = "2",
                IdProperty = "PROP002",
                Name = "Casa 2",
                Address = "Calle 2",
                Price = 200000,
                IdOwner = "OWNER002"
            }
        };

        _propertyRepositoryMock
            .Setup(x => x.GetPagedAsync(page, pageSize))
            .ReturnsAsync((properties, 2L));

        _mapperMock
            .Setup(x => x.Map<PropertyDto>(It.IsAny<Property>()))
            .Returns<Property>(p => new PropertyDto
            {
                IdProperty = p.IdProperty,
                Name = p.Name,
                Address = p.Address,
                Price = p.Price
            });

        _ownerRepositoryMock
            .Setup(x => x.GetOwnersByIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, Owner>());

        _propertyImageRepositoryMock
            .Setup(x => x.GetFirstEnabledImagesByPropertyIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, PropertyImage>());

        // Act
        var result = await _propertyService.GetAllPropertiesPagedAsync(page, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
    }

    [Test]
    public async Task SearchPropertiesPagedAsync_WithFilters_ReturnsFilteredResults()
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

        var properties = new List<Property>
        {
            new Property
            {
                Id = "1",
                IdProperty = "PROP001",
                Name = "Casa de Prueba",
                Address = "Calle Test",
                Price = 200000,
                IdOwner = "OWNER001"
            }
        };

        _propertyRepositoryMock
            .Setup(x => x.SearchPagedAsync(
                filter.Name,
                filter.Address,
                filter.MinPrice,
                filter.MaxPrice,
                page,
                pageSize))
            .ReturnsAsync((properties, 1L));

        _mapperMock
            .Setup(x => x.Map<PropertyDto>(It.IsAny<Property>()))
            .Returns<Property>(p => new PropertyDto
            {
                IdProperty = p.IdProperty,
                Name = p.Name,
                Address = p.Address,
                Price = p.Price
            });

        _ownerRepositoryMock
            .Setup(x => x.GetOwnersByIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, Owner>());

        _propertyImageRepositoryMock
            .Setup(x => x.GetFirstEnabledImagesByPropertyIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, PropertyImage>());

        // Act
        var result = await _propertyService.SearchPropertiesPagedAsync(filter, page, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
        _propertyRepositoryMock.Verify(
            x => x.SearchPagedAsync(
                filter.Name,
                filter.Address,
                filter.MinPrice,
                filter.MaxPrice,
                page,
                pageSize),
            Times.Once);
    }
}

