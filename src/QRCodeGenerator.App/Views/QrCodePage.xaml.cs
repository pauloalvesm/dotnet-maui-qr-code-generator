using QRCodeGenerator.App.Models;
using QRCodeGenerator.App.Services;

namespace QRCodeGenerator.App.Views;

public partial class QrCodePage : ContentPage
{
	private readonly QrCodeService _qrCodeService;

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

				byte[] imageBytes = Convert.FromBase64String(base64Pure);

				QrCodeImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
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
}