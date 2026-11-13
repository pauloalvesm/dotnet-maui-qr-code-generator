using QRCodeGenerator.App.Constants;
using QRCodeGenerator.App.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace QRCodeGenerator.App.Services;

public class QrCodeService
{
    private readonly HttpClient _httpClient;

    public QrCodeService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(QrCodeConstants.ApiBaseUrl);
        }
    }

    public async Task<string?> GenerateQrCodeAsync(QrCodeModel qrCodeModel)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/QrCode", qrCodeModel);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (jsonResponse.TryGetProperty("imageBase64", out var imageElement))
                {
                    return imageElement.GetString();
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error communicating with the API: {ex.Message}");
            throw;
        }
    }
}
