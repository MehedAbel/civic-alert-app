using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CivicAlert.DTOs.Report;

public class CreateReportRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    [FromForm(Name = "report")]
    public string? ReportJson { get; set; }

    public IFormFileCollection? Images { get; set; }

    public (string Title, string Description, string Category, double? Latitude, double? Longitude) Resolve()
    {
        if (!string.IsNullOrWhiteSpace(ReportJson))
        {
            try
            {
                var doc = JsonDocument.Parse(ReportJson);
                var root = doc.RootElement;
                return (
                    root.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
                    root.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "",
                    root.TryGetProperty("category", out var c) ? c.GetString() ?? "" : "",
                    root.TryGetProperty("latitude", out var lat) && lat.TryGetDouble(out var l1) ? l1 : null,
                    root.TryGetProperty("longitude", out var lon) && lon.TryGetDouble(out var l2) ? l2 : null
                );
            }
            catch { }
        }

        return (Title ?? "", Description ?? "", Category ?? "", Latitude, Longitude);
    }
}
