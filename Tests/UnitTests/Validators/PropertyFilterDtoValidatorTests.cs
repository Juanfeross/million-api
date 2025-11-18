using Core.Application.DTOs;
using Core.Application.Validators;
using FluentAssertions;
using FluentValidation;

namespace MillionBack.Tests.Validators;

[TestFixture]
public class PropertyFilterDtoValidatorTests
{
    private PropertyFilterDtoValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new PropertyFilterDtoValidator();
    }

    [Test]
    public void Validate_WhenMinPriceIsNegative_ShouldFail()
    {
        // Arrange
        var filter = new PropertyFilterDto
        {
            MinPrice = -100
        };

        // Act
        var result = _validator.Validate(filter);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MinPrice");
    }

    [Test]
    public void Validate_WhenMaxPriceIsNegative_ShouldFail()
    {
        // Arrange
        var filter = new PropertyFilterDto
        {
            MaxPrice = -100
        };

        // Act
        var result = _validator.Validate(filter);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MaxPrice");
    }

    [Test]
    public void Validate_WhenMaxPriceIsLessThanMinPrice_ShouldFail()
    {
        // Arrange
        var filter = new PropertyFilterDto
        {
            MinPrice = 200000,
            MaxPrice = 100000
        };

        // Act
        var result = _validator.Validate(filter);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == "MaxPrice" && 
            e.ErrorMessage.Contains("mayor o igual al precio mínimo"));
    }

    [Test]
    public void Validate_WhenMinPriceIsZero_ShouldPass()
    {
        // Arrange
        var filter = new PropertyFilterDto
        {
            MinPrice = 0
        };

        // Act
        var result = _validator.Validate(filter);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_WhenMaxPriceIsGreaterThanMinPrice_ShouldPass()
    {
        // Arrange
        var filter = new PropertyFilterDto
        {
            MinPrice = 100000,
            MaxPrice = 200000
        };

        // Act
        var result = _validator.Validate(filter);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_WhenMinPriceEqualsMaxPrice_ShouldPass()
    {
        // Arrange
        var filter = new PropertyFilterDto
        {
            MinPrice = 150000,
            MaxPrice = 150000
        };

        // Act
        var result = _validator.Validate(filter);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_WhenNoPriceFilters_ShouldPass()
    {
        // Arrange
        var filter = new PropertyFilterDto
        {
            Name = "Casa",
            Address = "Madrid"
        };

        // Act
        var result = _validator.Validate(filter);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_WhenAllFiltersAreValid_ShouldPass()
    {
        // Arrange
        var filter = new PropertyFilterDto
        {
            Name = "Casa",
            Address = "Madrid",
            MinPrice = 100000,
            MaxPrice = 500000
        };

        // Act
        var result = _validator.Validate(filter);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

