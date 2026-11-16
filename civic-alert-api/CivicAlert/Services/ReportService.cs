using CivicAlert.Data;
using CivicAlert.DTOs.Report;
using CivicAlert.Entities;
using CivicAlert.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CivicAlert.Services;

public class ReportService(CivicAlertDbContext db, IConfiguration config) : IReportService
{
    private readonly string _uploadPath = config["UploadSettings:UploadPath"] ?? "uploads";

    public async Task<ReportModel> CreateReportAsync(CreateReportRequest request, string username)
    {
        var appUser = await db.Users.FirstOrDefaultAsync(u => u.Email == username)
            ?? throw new InvalidLoginCredentialsException("User not found for report submission");

        var (title, description, category, latitude, longitude) = request.Resolve();
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidRegisterCredentialsException("Report title is required.");
        }

        if (!Directory.Exists(_uploadPath)) Directory.CreateDirectory(_uploadPath);

        var imagePaths = new List<string>();
        if (request.Images is { Count: > 0 })
        {
            foreach (var img in request.Images)
            {
                if (img.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(img.FileName)}";
                    var filePath = Path.Combine(_uploadPath, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await img.CopyToAsync(stream);
                    imagePaths.Add(filePath);
                }
            }
        }

        var report = new ReportModel
        {
            Title = title,
            Description = description,
            Category = category,
            Latitude = latitude,
            Longitude = longitude,
            CreatedAt = DateTime.UtcNow,
            Status = ReportStatus.OPEN,
            UserId = appUser.Id,
            User = appUser,
            ImagePaths = imagePaths
        };

        db.Reports.Add(report);
        await db.SaveChangesAsync();
        return report;
    }

    public async Task<ReportModel> GetReportByIdAsync(int reportId)
    {
        var report = await db.Reports.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == reportId);
        return report ?? throw new ReportNotFoundException("Report not found");
    }

    public async Task<List<ReportDto>> GetAllReportsAsync(string baseUrl)
    {
        var list = await db.Reports.Include(r => r.User).OrderByDescending(r => r.CreatedAt).ToListAsync();
        return list.Select(r => MapToDto(r, baseUrl)).ToList();
    }

    public async Task<List<ReportDto>> GetReportsByUsernameAsync(string username, string baseUrl)
    {
        var list = await db.Reports.Include(r => r.User)
            .Where(r => r.User != null && r.User.Email == username)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
        return list.Select(r => MapToDto(r, baseUrl)).ToList();
    }

    public async Task<List<ReportDto>> GetReportsByCategoryAsync(string category, string baseUrl)
    {
        var list = await db.Reports.Include(r => r.User)
            .Where(r => r.Category == category)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
        return list.Select(r => MapToDto(r, baseUrl)).ToList();
    }

    public async Task<string> DeleteReportAsync(int reportId)
    {
        var report = await db.Reports.FirstOrDefaultAsync(r => r.Id == reportId)
            ?? throw new ReportNotFoundException("Report not found");

        db.Reports.Remove(report);
        await db.SaveChangesAsync();
        return "Report deleted successfully";
    }

    private static ReportDto MapToDto(ReportModel r, string baseUrl)
    {
        var cleanBase = baseUrl.TrimEnd('/');
        var urls = r.ImagePaths.Select(p => $"{cleanBase}/uploads/{Path.GetFileName(p)}").ToList();
        return new ReportDto
        {
            Id = r.Id,
            Title = r.Title,
            Description = r.Description,
            Category = r.Category,
            Latitude = r.Latitude,
            Longitude = r.Longitude,
            ImageUrls = urls,
            Username = r.User?.Email ?? "",
            CreatedAt = r.CreatedAt,
            Status = r.Status
        };
    }
}
