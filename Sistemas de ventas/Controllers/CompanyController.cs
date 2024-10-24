using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/v1/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly AppDbContext _context;

    public CompanyController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCompanies()
    {
        return Ok(await _context.Companies.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompany(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        return company != null ? Ok(company) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCompany([FromBody] Company newCompany)
    {
        _context.Companies.Add(newCompany);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCompany), new { id = newCompany.Id }, newCompany);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(int id, [FromBody] Company updatedCompany)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return NotFound();

        company.Name = updatedCompany.Name;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return NotFound();

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
