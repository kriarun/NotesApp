using Microsoft.AspNetCore.Mvc;
using TargetApi.Mock.Models;

namespace TargetApi.Mock.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
{
    // In-memory store
    private static readonly List<Contract> _contracts = new();
    private static readonly List<ActivationCode> _activationCodes = new();
    private static readonly List<Letter> _letters = new();

    // POST /api/contracts
    [HttpPost]
    public ActionResult<Contract> CreateContract(Contract contract)
    {
        contract.Id = Guid.NewGuid().ToString();
        contract.CreatedAt = DateTime.UtcNow;
        contract.Status = "Created";
        _contracts.Add(contract);
        return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, contract);
    }

    // GET /api/contracts/{id}
    [HttpGet("{id}")]
    public ActionResult<Contract> GetContract(string id)
    {
        var contract = _contracts.FirstOrDefault(c => c.Id == id);
        if (contract is null) return NotFound();
        return Ok(contract);
    }

    // POST /api/contracts/{id}/activation
    [HttpPost("{id}/activation")]
    public ActionResult<ActivationCode> SendActivationCode(string id)
    {
        var contract = _contracts.FirstOrDefault(c => c.Id == id);
        if (contract is null) return NotFound();

        var code = new ActivationCode
        {
            Code = Guid.NewGuid().ToString()[..8].ToUpper(),
            ContractId = id,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _activationCodes.Add(code);
        return Ok(code);
    }

    // DELETE /api/contracts/{id}/activation
    [HttpDelete("{id}/activation")]
    public ActionResult DeleteActivationCode(string id)
    {
        var code = _activationCodes.FirstOrDefault(c => c.ContractId == id);
        if (code is null) return NotFound();

        _activationCodes.Remove(code);
        return NoContent();
    }

    // POST /api/contracts/{id}/letters
    [HttpPost("{id}/letters")]
    public ActionResult<Letter> SendLetter(string id, Letter letter)
    {
        var contract = _contracts.FirstOrDefault(c => c.Id == id);
        if (contract is null) return NotFound();

        letter.Id = Guid.NewGuid().ToString();
        letter.ContractId = id;
        letter.SentAt = DateTime.UtcNow;
        _letters.Add(letter);
        return Ok(letter);
    }
}