using System;

namespace Authorization.Domain;

public class Photo
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
}
