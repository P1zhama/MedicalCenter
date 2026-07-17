using System;


namespace Authorization.Domain;

public class AccountRole
{
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
