using System.Text.Json.Serialization;

namespace CivicAlert.Entities;

public class AppUser
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [JsonIgnore]
    public string Password { get; set; } = string.Empty;

    public HashSet<Role> Roles { get; set; } = [Role.CLIENT];

    [JsonIgnore]
    public List<ReportModel> Reports { get; set; } = [];
}
