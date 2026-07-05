using Authorization.Domain.Enums;


namespace Authorization.Domain;


public class Account
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public Guid? PhotoId { get; set; }

    public UserRole Role { get; set; } = UserRole.None;


    public string CreatedBy { get; set; } = "System";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string UpdatedBy { get; set; } = "System";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
