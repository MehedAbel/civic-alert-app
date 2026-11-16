using CivicAlert.Entities;

namespace CivicAlert.Services;

public interface IJwtService
{
    string GenerateToken(string username, IEnumerable<Role> roles);
}
