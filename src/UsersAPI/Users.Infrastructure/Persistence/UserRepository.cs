using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Users.Application.Interfaces;

namespace Users.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? "Data Source=users.db";

        EnsureSchema();
    }

    private void EnsureSchema()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS users (
  id TEXT PRIMARY KEY,
  name TEXT NOT NULL,
  email TEXT NOT NULL UNIQUE,
  password_hash TEXT NOT NULL,
  created_at_utc TEXT NOT NULL
);";
        cmd.ExecuteNonQuery();
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct)
    {
        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM users WHERE email = $email LIMIT 1;";
        cmd.Parameters.AddWithValue("$email", email);

        var result = await cmd.ExecuteScalarAsync(ct);
        return result != null;
    }

    public async Task<Guid> CreateAsync(string name, string email, string passwordHash, CancellationToken ct)
    {
        var id = Guid.NewGuid();

        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO users (id, name, email, password_hash, created_at_utc)
VALUES ($id, $name, $email, $hash, $createdAt);";
        cmd.Parameters.AddWithValue("$id", id.ToString());
        cmd.Parameters.AddWithValue("$name", name);
        cmd.Parameters.AddWithValue("$email", email);
        cmd.Parameters.AddWithValue("$hash", passwordHash);
        cmd.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));

        await cmd.ExecuteNonQueryAsync(ct);
        return id;
    }

    public async Task<(Guid Id, string Name, string Email, string PasswordHash)?> GetByEmailAsync(string email, CancellationToken ct)
    {
        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, name, email, password_hash FROM users WHERE email = $email LIMIT 1;";
        cmd.Parameters.AddWithValue("$email", email);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        var id = Guid.Parse(reader.GetString(0));
        var name = reader.GetString(1);
        var em = reader.GetString(2);
        var hash = reader.GetString(3);

        return (id, name, em, hash);
    }
}
