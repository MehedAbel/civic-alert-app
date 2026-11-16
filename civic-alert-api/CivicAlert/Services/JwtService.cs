using CivicAlert.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CivicAlert.Services;

public class JwtService(IConfiguration config) : IJwtService
{
    private readonly string _key = config["Jwt:Key"] ?? "CivicAlert_Super_Secure_Secret_Key_For_Jwt_HMAC_SHA256_Auth_Token_2026_DotNet10";
    private readonly string _issuer = config["Jwt:Issuer"] ?? "CivicAlertApi";
    private readonly string _audience = config["Jwt:Audience"] ?? "CivicAlertClients";
    private readonly int _expiry = int.TryParse(config["Jwt:ExpiryMinutes"], out var exp) ? exp : 15000;

    public string GenerateToken(string username, IEnumerable<Role> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, username),
            new(JwtRegisteredClaimNames.Sub, username)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, "ROLE_" + role));
            claims.Add(new Claim("role", role.ToString()));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expiry),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };

        return tokenHandler.WriteToken(tokenHandler.CreateToken(descriptor));
    }
}
