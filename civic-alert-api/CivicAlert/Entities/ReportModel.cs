namespace CivicAlert.Entities;

public class ReportModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ReportStatus Status { get; set; } = ReportStatus.OPEN;
    public List<string> ImagePaths { get; set; } = [];

    public int UserId { get; set; }
    public AppUser? User { get; set; }
}
