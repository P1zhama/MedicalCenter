using Offices.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Offices.Domain;

public class Office
{
    public Guid Id { get; set; }

    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;

    public string? OfficeNumber { get; set; }

    public string RegistryPhoneNumber { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }

    public OfficeStatus Status { get; set; } = OfficeStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public string FormatAddress()
    {
        var parts = new List<string> { City, Street, HouseNumber };

        if (!string.IsNullOrWhiteSpace(OfficeNumber))
        {
            parts.Add(OfficeNumber);
        }

        return string.Join(", ", parts);
    }
}
