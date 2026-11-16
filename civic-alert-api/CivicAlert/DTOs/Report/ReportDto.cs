using CivicAlert.Entities;

namespace CivicAlert.DTOs.Report;

public class ReportDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public List<string> ImageUrls { get; set; } = [];
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ReportStatus Status { get; set; }
}
