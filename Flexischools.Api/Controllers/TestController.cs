using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flexischools.Infrastructure.Data;

namespace Flexischools.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly FlexischoolsDbContext _context;

    public TestController(FlexischoolsDbContext context)
    {
        _context = context;
    }

    [HttpGet("database")]
    public async Task<ActionResult> TestDatabaseConnection()
    {
        try
        {
            // Test database connection
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return BadRequest(new { message = "Cannot connect to database" });
            }

            // Test basic queries
            var parentCount = await _context.Parents.CountAsync();
            var studentCount = await _context.Students.CountAsync();
            var canteenCount = await _context.Canteens.CountAsync();
            var menuItemCount = await _context.MenuItems.CountAsync();

            return Ok(new
            {
                message = "Database connection successful",
                data = new
                {
                    parents = parentCount,
                    students = studentCount,
                    canteens = canteenCount,
                    menuItems = menuItemCount
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Database error: {ex.Message}" });
        }
    }

    [HttpGet("parents")]
    public async Task<ActionResult> GetParents()
    {
        try
        {
            var parents = await _context.Parents
                .Include(p => p.Students)
                .ToListAsync();

            return Ok(parents);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("menu-items")]
    public async Task<ActionResult> GetMenuItems()
    {
        try
        {
            var menuItems = await _context.MenuItems
                .Include(mi => mi.Canteen)
                .ToListAsync();

            return Ok(menuItems);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
