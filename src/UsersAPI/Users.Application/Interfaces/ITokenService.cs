using System;

namespace Users.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(Guid userId, string email, string name);
}
