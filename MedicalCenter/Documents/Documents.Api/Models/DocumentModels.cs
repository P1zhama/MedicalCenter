using Microsoft.AspNetCore.Http;

namespace Documents.Api.Models;

public sealed class UploadPhotoRequest
{
    public IFormFile? File { get; set; }

    public string Kind { get; set; } = string.Empty;

    public Guid? OwnerProfileId { get; set; }
}
