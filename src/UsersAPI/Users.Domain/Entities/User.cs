using System;

namespace Users.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;

    // Necessário para o EF/UsersDbContext
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
