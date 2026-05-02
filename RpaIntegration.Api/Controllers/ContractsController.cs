using Microsoft.AspNetCore.Mvc;
using RpaIntegration.Api.Models;
using RpaIntegration.Api.Services;

namespace RpaIntegration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpPost]
    public async Task<ActionResult<Contract>> CreateContract(Contract contract)
    {
        var created = await _contractService.CreateContractAsync(contract);
        return CreatedAtAction(nameof(GetContract), new { id = created.Id }, created);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Contract>> GetContract(string id)
    {
        var contract = await _contractService.GetContractAsync(id);
        return Ok(contract);
    }

    [HttpPost("{id}/activation")]
    public async Task<ActionResult<ActivationCode>> SendActivationCode(string id)
    {
        var code = await _contractService.SendActivationCodeAsync(id);
        return Ok(code);
    }

    [HttpDelete("{id}/activation")]
    public async Task<ActionResult> DeleteActivationCode(string id)
    {
        await _contractService.DeleteActivationCodeAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/letters")]
    public async Task<ActionResult<Letter>> SendLetter(string id, Letter letter)
    {
        var sent = await _contractService.SendLetterAsync(id, letter);
        return Ok(sent);
    }
}