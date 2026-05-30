using DFIR.CaseManagement.Auth;
using DFIR.CaseManagement.DTOs;
using DFIR.CaseManagement.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFIR.CaseManagement.Controllers;

[ApiController]
[Route("api/evidence")]
[Authorize]
public class EvidenceController : ControllerBase
{
    private readonly IEvidenceService _evidence;
    private readonly IValidator<EvidenceCreateDto> _validator;

    public EvidenceController(IEvidenceService evidence, IValidator<EvidenceCreateDto> validator)
    {
        _evidence = evidence;
        _validator = validator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EvidenceDto>>> GetAll()
        => Ok(await _evidence.GetAllAsync());

    [HttpGet("case/{caseId:int}")]
    public async Task<ActionResult<IReadOnlyList<EvidenceDto>>> GetByCase(int caseId)
        => Ok(await _evidence.GetByCaseAsync(caseId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EvidenceDto>> GetById(int id)
    {
        var result = await _evidence.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.EvidenceWrite)]
    public async Task<ActionResult<EvidenceDto>> Create([FromBody] EvidenceCreateDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var created = await _evidence.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.EvidenceWrite)]
    public async Task<ActionResult<EvidenceDto>> Update(int id, [FromBody] EvidenceCreateDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var updated = await _evidence.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.EvidenceDelete)]
    public async Task<IActionResult> Delete(int id)
        => await _evidence.DeleteAsync(id) ? NoContent() : NotFound();

    /// <summary>Re-hashes the uploaded file and verifies it against the stored evidence hash.</summary>
    [HttpPost("{id:int}/verify")]
    [Authorize(Policy = Permissions.EvidenceWrite)]
    public async Task<ActionResult<object>> Verify(int id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "A file is required for verification." });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var match = await _evidence.VerifyHashesAsync(id, ms.ToArray());
        return Ok(new { evidenceId = id, integrityVerified = match });
    }
}
