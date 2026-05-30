using DFIR.CaseManagement.Auth;
using DFIR.CaseManagement.DTOs;
using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFIR.CaseManagement.Controllers;

[ApiController]
[Route("api/cases")]
[Authorize]
public class CasesController : ControllerBase
{
    private readonly ICaseService _cases;
    private readonly IValidator<CaseCreateDto> _createValidator;
    private readonly IValidator<CaseUpdateDto> _updateValidator;

    public CasesController(
        ICaseService cases,
        IValidator<CaseCreateDto> createValidator,
        IValidator<CaseUpdateDto> updateValidator)
    {
        _cases = cases;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CaseDto>>> GetAll()
        => Ok(await _cases.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CaseDto>> GetById(int id)
    {
        var result = await _cases.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.CasesWrite)]
    public async Task<ActionResult<CaseDto>> Create([FromBody] CaseCreateDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var created = await _cases.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.CasesWrite)]
    public async Task<ActionResult<CaseDto>> Update(int id, [FromBody] CaseUpdateDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var updated = await _cases.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.CasesDelete)]
    public async Task<IActionResult> Delete(int id)
        => await _cases.DeleteAsync(id) ? NoContent() : NotFound();

    /// <summary>Advance the case along the ICAPAIR workflow.</summary>
    [HttpPatch("{id:int}/stage")]
    [Authorize(Policy = Permissions.CasesWrite)]
    public async Task<ActionResult<CaseDto>> AdvanceStage(int id, [FromQuery] WorkflowStage stage)
    {
        var result = await _cases.AdvanceStageAsync(id, stage);
        return result is null ? NotFound() : Ok(result);
    }
}
