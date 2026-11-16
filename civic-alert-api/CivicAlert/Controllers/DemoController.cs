using CivicAlert.Data;
using CivicAlert.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivicAlert.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController(CivicAlertDbContext db) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(DemoModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> TestDemo([FromBody] DemoModel model)
    {
        db.Demos.Add(model);
        await db.SaveChangesAsync();
        return StatusCode(StatusCodes.Status201Created, model);
    }

    [Authorize(Roles = "ROLE_CLIENT,CLIENT")]
    [HttpGet]
    [ProducesResponseType(typeof(List<DemoModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDemos()
    {
        var demos = await db.Demos.ToListAsync();
        return Ok(demos);
    }
}
