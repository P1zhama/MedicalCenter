using System.Collections.Generic;

namespace Authorization.Domain;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();
}