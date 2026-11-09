using QRCodeGenerator.App.Models;
using QRCodeGenerator.App.Services;

namespace QRCodeGenerator.App.Views;

public partial class QrCodePage : ContentPage
{
    private readonly QrCodeService _qrCodeService;
    private byte[]? _currentImageBytes;

    public QrCodePage(QrCodeService qrCodeService)
    {
        InitializeComponent();
        _qrCodeService = qrCodeService;
    }

    private async void OnGenerateClicked(object sender, EventArgs e)
    {
        var inputText = ContentEntry.Text;

        if (string.IsNullOrWhiteSpace(inputText))
        {
            await DisplayAlertAsync("Attention", "The QR Code content is mandatory.", "OK");
            return;
        }

        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var qrCodeModel = new QrCodeModel { Content = inputText };
            var base64DataUri = await _qrCodeService.GenerateQrCodeAsync(qrCodeModel);

            if (!string.IsNullOrEmpty(base64DataUri))
            {
                var base64Pure = base64DataUri.Contains(",")
                    ? base64DataUri.Split(',')[1]
                    : base64DataUri;

                _currentImageBytes = Convert.FromBase64String(base64Pure);

                QrCodeImage.Source = ImageSource.FromStream(() => new MemoryStream(_currentImageBytes));

                DownloadButton.IsVisible = true;
            }
            else
            {
                await DisplayAlertAsync("Error", "Could not generate the QR Code.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"An error occurred: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnDownloadClicked(object sender, EventArgs e)
    {
        if (_currentImageBytes == null || _currentImageBytes.Length == 0)
        {
            await DisplayAlertAsync("Attention", "There is no QR Code to download.", "OK");
            return;
        }

        try
        {
            var fileName = $"QRCode_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var targetFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            File.WriteAllBytes(targetFilePath, _currentImageBytes);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Save QR Code",
                File = new ShareFile(targetFilePath)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Could not save the file: {ex.Message}", "OK");
        }
    }
}