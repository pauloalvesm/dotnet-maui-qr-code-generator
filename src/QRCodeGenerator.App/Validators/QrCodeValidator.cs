namespace QRCodeGenerator.App.Validators;

public static class QrCodeValidator
{
    public static bool ValidateContent(string? content, out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            errorMessage = "The QR Code content cannot be empty or null.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
