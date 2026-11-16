using CivicAlert.DTOs.Report;
using CivicAlert.Entities;
using CivicAlert.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CivicAlert.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController(IReportService reportService) : ControllerBase
{
    [Authorize(Roles = "ROLE_CLIENT,CLIENT")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ReportModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateReport([FromForm] CreateReportRequest request)
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.Identity?.Name;

        if (string.IsNullOrEmpty(username)) return Unauthorized();

        var created = await reportService.CreateReportAsync(request, username);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpGet("{reportId:int}")]
    [ProducesResponseType(typeof(ReportModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReport(int reportId)
    {
        var report = await reportService.GetReportByIdAsync(reportId);
        return Ok(report);
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(List<ReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllReports()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var reports = await reportService.GetAllReportsAsync(baseUrl);
        return Ok(reports);
    }

    [HttpGet("all-by-username/{username}")]
    [ProducesResponseType(typeof(List<ReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllReportsByUsername(string username)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var reports = await reportService.GetReportsByUsernameAsync(username, baseUrl);
        return Ok(reports);
    }

    [HttpGet("all-by-category/{category}")]
    [ProducesResponseType(typeof(List<ReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllReportsByCategory(string category)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var reports = await reportService.GetReportsByCategoryAsync(category, baseUrl);
        return Ok(reports);
    }

    [HttpDelete("{reportId:int}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReport(int reportId)
    {
        var message = await reportService.DeleteReportAsync(reportId);
        return Ok(message);
    }
}
