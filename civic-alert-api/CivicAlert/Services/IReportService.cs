using CivicAlert.DTOs.Report;
using CivicAlert.Entities;

namespace CivicAlert.Services;

public interface IReportService
{
    Task<ReportModel> CreateReportAsync(CreateReportRequest request, string username);
    Task<ReportModel> GetReportByIdAsync(int reportId);
    Task<List<ReportDto>> GetAllReportsAsync(string baseUrl);
    Task<List<ReportDto>> GetReportsByUsernameAsync(string username, string baseUrl);
    Task<List<ReportDto>> GetReportsByCategoryAsync(string category, string baseUrl);
    Task<string> DeleteReportAsync(int reportId);
}
