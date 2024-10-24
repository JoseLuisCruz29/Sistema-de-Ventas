using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/v2/[controller]")]
public class EmployeeV2Controller : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeeV2Controller(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployees()
    {
        var employees = await _context.Employees.ToListAsync();
        var result = employees.Select(e => new
        {
            e.Id,
            FullName = $"{e.Name} - {e.Position}",
            e.Salary
        });
        return Ok(result);
    }
}
