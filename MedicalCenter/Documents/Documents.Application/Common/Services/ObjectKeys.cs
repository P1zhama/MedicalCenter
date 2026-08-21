namespace Documents.Application.Common.Services;

public static class ObjectKeys
{
    public static string ForPhoto(Guid id, string extension, DateTimeOffset now)
        => $"photos/{now:yyyy}/{now:MM}/{id:N}{extension}";
}
