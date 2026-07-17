using System;

namespace Profiles.Domain;

public class Receptionist
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }


    public Guid AccountId { get; set; }
    public Guid OfficeId { get; set; }

    public string? PhotoUrl { get; set; }
}