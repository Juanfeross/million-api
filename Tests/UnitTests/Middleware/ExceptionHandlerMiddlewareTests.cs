using System.Net;
using System.Text;
using System.Text.Json;
using API.Middleware;
using API.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace MillionBack.Tests.Middleware;

[TestFixture]
public class ExceptionHandlerMiddlewareTests
{
    private Mock<RequestDelegate> _nextMock = null!;
    private Mock<ILogger<ExceptionHandlerMiddleware>> _loggerMock = null!;
    private ExceptionHandlerMiddleware _middleware = null!;
    private DefaultHttpContext _httpContext = null!;

    [SetUp]
    public void SetUp()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<ExceptionHandlerMiddleware>>();
        _middleware = new ExceptionHandlerMiddleware(_nextMock.Object, _loggerMock.Object);
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [Test]
    public async Task InvokeAsync_WhenNoException_DoesNotHandleException()
    {
        // Arrange
        _nextMock
            .Setup(x => x(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(200);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WhenGenericException_ReturnsInternalServerError()
    {
        // Arrange
        var exception = new Exception("Generic error");
        _nextMock
            .Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        _httpContext.Response.ContentType.Should().Be("application/json");

        var responseBody = await GetResponseBody();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(500);
        errorResponse.Message.Should().Be("Error interno del servidor");
        errorResponse.Details.Should().Be("Generic error");
    }

    [Test]
    public async Task InvokeAsync_WhenArgumentNullException_ReturnsBadRequest()
    {
        // Arrange
        var exception = new ArgumentNullException("param", "Parameter is null");
        _nextMock
            .Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

        var responseBody = await GetResponseBody();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(400);
        errorResponse.Message.Should().Be("Parámetro requerido no proporcionado");
    }

    [Test]
    public async Task InvokeAsync_WhenArgumentException_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument value");
        _nextMock
            .Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

        var responseBody = await GetResponseBody();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(400);
        errorResponse.Message.Should().Be("Invalid argument value");
    }

    [Test]
    public async Task InvokeAsync_WhenKeyNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var exception = new KeyNotFoundException("Resource not found");
        _nextMock
            .Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

        var responseBody = await GetResponseBody();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(404);
        errorResponse.Message.Should().Be("Recurso no encontrado");
    }

    [Test]
    public async Task InvokeAsync_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        var exception = new Exception("Test error");
        _nextMock
            .Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_WhenExceptionOccurs_ResponseHasTimestamp()
    {
        // Arrange
        var exception = new Exception("Test error");
        _nextMock
            .Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBody();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    private async Task<string> GetResponseBody()
    {
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_httpContext.Response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}

