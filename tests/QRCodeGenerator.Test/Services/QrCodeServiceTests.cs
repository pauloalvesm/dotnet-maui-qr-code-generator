using System.Net;
using Moq;
using Moq.Protected;
using QRCodeGenerator.App.Models;
using QRCodeGenerator.App.Services;

namespace QRCodeGenerator.Test.Services;

public class QrCodeServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly QrCodeService _qrCodeService;

    public QrCodeServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5268/")
        };

        _qrCodeService = new QrCodeService(_httpClient);
    }

    [Fact]
    public async Task GenerateQrCodeAsync_ShouldReturnBase64_WhenApiReturnsSuccessAndValidJson()
    {
        // Arrange
        var model = new QrCodeModel {};
        var jsonResponse = "{\"imageBase64\": \"iVBORw0KGgoAAAANSUhEUg...\"}";

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _qrCodeService.GenerateQrCodeAsync(model);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("iVBORw0KGgoAAAANSUhEUg...", result);
    }

    [Fact]
    public async Task GenerateQrCodeAsync_ShouldReturnNull_WhenApiReturnsNonSuccessStatusCode()
    {
        // Arrange
        var model = new QrCodeModel();
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _qrCodeService.GenerateQrCodeAsync(model);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GenerateQrCodeAsync_ShouldReturnNull_WhenJsonDoesNotHaveImageBase64Property()
    {
        // Arrange
        var model = new QrCodeModel();
        var jsonResponse = "{\"otherProperty\": \"value\"}";

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _qrCodeService.GenerateQrCodeAsync(model);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GenerateQrCodeAsync_ShouldThrowException_WhenNetworkFailureOccurs()
    {
        // Arrange
        var model = new QrCodeModel();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network failure"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _qrCodeService.GenerateQrCodeAsync(model));
    }
}
