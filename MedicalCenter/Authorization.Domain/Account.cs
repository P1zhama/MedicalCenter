using System;
using System.Collections.Generic;


namespace Authorization.Domain;


public class Account
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsEmailVerified { get; set; } = false;

    public bool IsProfileCreated { get; set; } = false;

    public Photo? Photo { get; set; }

    public ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();

    
    public string? CreatedBy { get; set; } // Email
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; } // Email
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

}
