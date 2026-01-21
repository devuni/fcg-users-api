using System;
using System.Threading;
using System.Threading.Tasks;

namespace Users.Application.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);
    Task<Guid> CreateAsync(string name, string email, string passwordHash, CancellationToken ct);

    Task<(Guid Id, string Name, string Email, string PasswordHash)?> GetByEmailAsync(
        string email,
        CancellationToken ct);
}
