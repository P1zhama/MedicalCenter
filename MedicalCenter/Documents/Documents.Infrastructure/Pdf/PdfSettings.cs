namespace Documents.Infrastructure.Pdf;

public sealed class PdfSettings
{
    public const string SectionName = "PdfSettings";

    public string RegularFontPath { get; set; } = string.Empty;

    public string BoldFontPath { get; set; } = string.Empty;

    public string FontFamily { get; set; } = string.Empty;

    public string ClinicName { get; set; } = string.Empty;
}
