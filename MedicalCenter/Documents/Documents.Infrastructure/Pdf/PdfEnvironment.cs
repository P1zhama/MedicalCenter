using QuestPDF.Drawing;
using QuestPDF.Infrastructure;

namespace Documents.Infrastructure.Pdf;

public static class PdfEnvironment
{
    public static void Configure(PdfSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.FontFamily))
            throw new InvalidOperationException("PDF font family is not configured.");

        QuestPDF.Settings.License = LicenseType.Community;

        RegisterFont(settings.RegularFontPath);
        RegisterFont(settings.BoldFontPath);
    }

    private static void RegisterFont(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("PDF font path is not configured.");

        if (!File.Exists(path))
            throw new InvalidOperationException($"PDF font file '{path}' was not found.");

        using var stream = File.OpenRead(path);

        FontManager.RegisterFont(stream);
    }
}
