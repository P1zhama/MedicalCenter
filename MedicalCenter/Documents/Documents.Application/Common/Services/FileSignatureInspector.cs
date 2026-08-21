namespace Documents.Application.Common.Services;

public sealed record FileSignature(string ContentType, string Extension);

public static class FileSignatureInspector
{
    public const int HeaderLength = 12;

    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF];

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static readonly byte[] Riff = [0x52, 0x49, 0x46, 0x46];

    private static readonly byte[] Webp = [0x57, 0x45, 0x42, 0x50];

    public static FileSignature? Detect(ReadOnlySpan<byte> header)
    {
        if (StartsWith(header, Jpeg))
            return new FileSignature("image/jpeg", ".jpg");

        if (StartsWith(header, Png))
            return new FileSignature("image/png", ".png");

        if (StartsWith(header, Riff) && header.Length >= 12 && header[8..12].SequenceEqual(Webp))
            return new FileSignature("image/webp", ".webp");

        return null;
    }

    private static bool StartsWith(ReadOnlySpan<byte> header, ReadOnlySpan<byte> signature)
        => header.Length >= signature.Length && header[..signature.Length].SequenceEqual(signature);
}
