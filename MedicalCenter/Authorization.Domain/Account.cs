using Authorization.Domain.Enums;


namespace Authorization.Domain;


public class Account
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public Photo? Photo { get; set; }

    public UserRole Role { get; set; } = UserRole.None;


    public UserRole CreatedBy { get; set; } = UserRole.None;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public UserRole UpdatedBy { get; set; } = UserRole.None;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
