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
public class PropertyServiceAdditionalTests
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
    public async Task GetAllPropertiesPagedAsync_WhenResultIsCached_ReturnsCachedResult()
    {
        // Arrange
        var page = 1;
        var pageSize = 20;
        var cachedResult = new PagedResult<PropertyDto>
        {
            Items = new List<PropertyDto> { new PropertyDto { IdProperty = "PROP001", Name = "Cached" } },
            Total = 1,
            Page = page,
            PageSize = pageSize
        };

        var cacheKey = $"properties_page_{page}_size_{pageSize}";
        _memoryCache.Set(cacheKey, cachedResult, TimeSpan.FromMinutes(5));

        // Act
        var result = await _propertyService.GetAllPropertiesPagedAsync(page, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Cached");
        _propertyRepositoryMock.Verify(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task GetAllPropertiesPagedAsync_WhenNotCached_CallsRepositoryAndCachesResult()
    {
        // Arrange
        var page = 1;
        var pageSize = 20;
        var properties = new List<Property>
        {
            new Property { Id = "1", IdProperty = "PROP001", Name = "Casa 1", Price = 100000 }
        };

        _propertyRepositoryMock
            .Setup(x => x.GetPagedAsync(page, pageSize))
            .ReturnsAsync((properties, 1L));

        _mapperMock
            .Setup(x => x.Map<PropertyDto>(It.IsAny<Property>()))
            .Returns<Property>(p => new PropertyDto
            {
                IdProperty = p.IdProperty,
                Name = p.Name,
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
        _propertyRepositoryMock.Verify(x => x.GetPagedAsync(page, pageSize), Times.Once);

        // Verify cache was set
        var cacheKey = $"properties_page_{page}_size_{pageSize}";
        _memoryCache.TryGetValue(cacheKey, out PagedResult<PropertyDto>? cached).Should().BeTrue();
        cached.Should().NotBeNull();
    }

    [Test]
    public async Task SearchPropertiesPagedAsync_WhenResultIsCached_ReturnsCachedResult()
    {
        // Arrange
        var filter = new PropertyFilterDto { Name = "Casa" };
        var page = 1;
        var pageSize = 20;
        var cachedResult = new PagedResult<PropertyDto>
        {
            Items = new List<PropertyDto> { new PropertyDto { IdProperty = "PROP001", Name = "Cached Casa" } },
            Total = 1,
            Page = page,
            PageSize = pageSize
        };

        var cacheKey = $"search_name_Casa_addr_null_min_null_max_null_page_{page}_size_{pageSize}";
        _memoryCache.Set(cacheKey, cachedResult, TimeSpan.FromMinutes(5));

        // Act
        var result = await _propertyService.SearchPropertiesPagedAsync(filter, page, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        _propertyRepositoryMock.Verify(x => x.SearchPagedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task SearchPropertiesPagedAsync_WhenResultIsEmpty_DoesNotCache()
    {
        // Arrange
        var filter = new PropertyFilterDto { Name = "NonExistent" };
        var page = 1;
        var pageSize = 20;

        _propertyRepositoryMock
            .Setup(x => x.SearchPagedAsync(filter.Name, filter.Address, filter.MinPrice, filter.MaxPrice, page, pageSize))
            .ReturnsAsync((new List<Property>(), 0L));

        _mapperMock
            .Setup(x => x.Map<PropertyDto>(It.IsAny<Property>()))
            .Returns<Property>(p => new PropertyDto());

        _ownerRepositoryMock
            .Setup(x => x.GetOwnersByIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, Owner>());

        _propertyImageRepositoryMock
            .Setup(x => x.GetFirstEnabledImagesByPropertyIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, PropertyImage>());

        // Act
        var result = await _propertyService.SearchPropertiesPagedAsync(filter, page, pageSize);

        // Assert
        result.Items.Should().BeEmpty();
        var cacheKey = $"search_name_NonExistent_addr_null_min_null_max_null_page_{page}_size_{pageSize}";
        _memoryCache.TryGetValue(cacheKey, out _).Should().BeFalse();
    }

    [Test]
    public async Task GetPropertyByIdAsync_WhenPropertyHasOwner_IncludesOwnerInDto()
    {
        // Arrange
        var propertyId = "507f1f77bcf86cd799439011";
        var property = new Property
        {
            Id = propertyId,
            IdProperty = "PROP001",
            Name = "Casa de Prueba",
            IdOwner = "OWNER001"
        };

        var owner = new Owner
        {
            Id = "OWNER001",
            IdOwner = "OWNER001",
            Name = "Juan Pérez",
            Address = "Calle Owner 123",
            Photo = "photo.jpg",
            Birthday = new DateTime(1980, 1, 1)
        };

        var propertyDetailDto = new PropertyDetailDto
        {
            IdProperty = property.IdProperty,
            Name = property.Name
        };

        var ownerDto = new OwnerDto
        {
            IdOwner = owner.IdOwner,
            Name = owner.Name,
            Address = owner.Address,
            Photo = owner.Photo,
            Birthday = owner.Birthday
        };

        _propertyRepositoryMock
            .Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        _mapperMock
            .Setup(x => x.Map<PropertyDetailDto>(property))
            .Returns(propertyDetailDto);

        _mapperMock
            .Setup(x => x.Map<OwnerDto>(owner))
            .Returns(ownerDto);

        _ownerRepositoryMock
            .Setup(x => x.GetOwnersByIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, Owner> { { "OWNER001", owner } });

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
        result!.Owner.Should().NotBeNull();
        result.Owner!.Name.Should().Be("Juan Pérez");
        result.Owner.Address.Should().Be("Calle Owner 123");
    }

    [Test]
    public async Task GetPropertyByIdAsync_WhenPropertyHasImages_IncludesImagesInDto()
    {
        // Arrange
        var propertyId = "507f1f77bcf86cd799439011";
        var property = new Property
        {
            Id = propertyId,
            IdProperty = "PROP001",
            Name = "Casa de Prueba"
        };

        var images = new List<PropertyImage>
        {
            new PropertyImage { IdProperty = propertyId, File = "image1.jpg", Enabled = true },
            new PropertyImage { IdProperty = propertyId, File = "image2.jpg", Enabled = true }
        };

        var propertyDetailDto = new PropertyDetailDto
        {
            IdProperty = property.IdProperty,
            Name = property.Name
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
            .ReturnsAsync(new Dictionary<string, List<PropertyImage>> { { propertyId, images } });

        _propertyTraceRepositoryMock
            .Setup(x => x.GetTracesByPropertyIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, List<PropertyTrace>>());

        // Act
        var result = await _propertyService.GetPropertyByIdAsync(propertyId);

        // Assert
        result.Should().NotBeNull();
        result!.Images.Should().HaveCount(2);
        result.Images.Should().Contain("image1.jpg");
        result.Images.Should().Contain("image2.jpg");
    }

    [Test]
    public async Task GetPropertyByIdAsync_WhenPropertyHasTraces_IncludesTracesInDto()
    {
        // Arrange
        var propertyId = "507f1f77bcf86cd799439011";
        var property = new Property
        {
            Id = propertyId,
            IdProperty = "PROP001",
            Name = "Casa de Prueba"
        };

        var traces = new List<PropertyTrace>
        {
            new PropertyTrace
            {
                IdProperty = propertyId,
                DateSale = new DateTime(2023, 1, 15),
                Name = "Venta 1",
                Value = 200000,
                Tax = 20000
            },
            new PropertyTrace
            {
                IdProperty = propertyId,
                DateSale = new DateTime(2023, 6, 20),
                Name = "Venta 2",
                Value = 250000,
                Tax = 25000
            }
        };

        var propertyDetailDto = new PropertyDetailDto
        {
            IdProperty = property.IdProperty,
            Name = property.Name
        };

        _propertyRepositoryMock
            .Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        _mapperMock
            .Setup(x => x.Map<PropertyDetailDto>(property))
            .Returns(propertyDetailDto);

        _mapperMock
            .Setup(x => x.Map<PropertyTraceDto>(It.IsAny<PropertyTrace>()))
            .Returns<PropertyTrace>(t => new PropertyTraceDto
            {
                DateSale = t.DateSale,
                Name = t.Name,
                Value = t.Value,
                Tax = t.Tax
            });

        _ownerRepositoryMock
            .Setup(x => x.GetOwnersByIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, Owner>());

        _propertyImageRepositoryMock
            .Setup(x => x.GetEnabledImagesByPropertyIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, List<PropertyImage>>());

        _propertyTraceRepositoryMock
            .Setup(x => x.GetTracesByPropertyIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, List<PropertyTrace>> { { propertyId, traces } });

        // Act
        var result = await _propertyService.GetPropertyByIdAsync(propertyId);

        // Assert
        result.Should().NotBeNull();
        result!.Traces.Should().HaveCount(2);
        result.Traces.First().Name.Should().Be("Venta 1");
        result.Traces.Last().Name.Should().Be("Venta 2");
    }

    [Test]
    public async Task GetAllPropertiesPagedAsync_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        _propertyRepositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await _propertyService
            .Invoking(x => x.GetAllPropertiesPagedAsync(1, 20))
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("Database connection failed");
    }

    [Test]
    public async Task MapPropertiesToDtoAsync_WhenPropertiesHaveOwnersAndImages_MapsCorrectly()
    {
        // Arrange
        var properties = new List<Property>
        {
            new Property
            {
                Id = "1",
                IdProperty = "PROP001",
                Name = "Casa 1",
                IdOwner = "OWNER001"
            }
        };

        var owner = new Owner
        {
            Id = "OWNER001",
            IdOwner = "OWNER001",
            Name = "Juan Pérez"
        };

        var image = new PropertyImage
        {
            IdProperty = "PROP001",
            File = "image.jpg",
            Enabled = true
        };

        _propertyRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(properties);

        _mapperMock
            .Setup(x => x.Map<PropertyDto>(It.IsAny<Property>()))
            .Returns<Property>(p => new PropertyDto
            {
                IdProperty = p.IdProperty,
                Name = p.Name
            });

        _ownerRepositoryMock
            .Setup(x => x.GetOwnersByIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, Owner> { { "OWNER001", owner } });

        _propertyImageRepositoryMock
            .Setup(x => x.GetFirstEnabledImagesByPropertyIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, PropertyImage> { { "1", image } });

        // Act
        var result = await _propertyService.GetAllPropertiesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        var dto = result.First();
        dto.OwnerName.Should().Be("Juan Pérez");
        dto.Image.Should().Be("image.jpg");
    }
}

