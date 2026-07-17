
using System;

namespace Profiles.Domain;

public class Patient    
{
    public Guid Id { get; set; }
    public Guid? AccountId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }

    public string? PhoneNumber { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string? PhotoUrl { get; set; }


    public bool IsLinkedToAccount => AccountId.HasValue;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}