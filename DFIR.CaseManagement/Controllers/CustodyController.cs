using DFIR.CaseManagement.Auth;
using DFIR.CaseManagement.DTOs;
using DFIR.CaseManagement.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFIR.CaseManagement.Controllers;

[ApiController]
[Route("api/custody")]
[Authorize]
public class CustodyController : ControllerBase
{
    private readonly ICustodyService _custody;
    private readonly IValidator<CustodyCreateDto> _validator;

    public CustodyController(ICustodyService custody, IValidator<CustodyCreateDto> validator)
    {
        _custody = custody;
        _validator = validator;
    }

    [HttpGet("case/{caseId:int}")]
    public async Task<ActionResult<IReadOnlyList<CustodyDto>>> GetByCase(int caseId)
        => Ok(await _custody.GetByCaseAsync(caseId));

    [HttpPost]
    [Authorize(Policy = Permissions.CustodyWrite)]
    public async Task<ActionResult<CustodyDto>> Create([FromBody] CustodyCreateDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var created = await _custody.CreateAsync(dto);
        return Ok(created);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.CustodyWrite)]
    public async Task<IActionResult> Delete(int id)
        => await _custody.DeleteAsync(id) ? NoContent() : NotFound();
}
