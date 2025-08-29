using Microsoft.AspNetCore.Mvc;
using TicketSystemApi.Dtos;
using TicketSystemApi.Services;

namespace TicketSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService service) : ControllerBase
{
    private readonly IUserService _service = service;

     [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(Guid guid)
    {
        var result = await _service.GetByIdAsync(guid);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] UserCreateDto dto)
    {

        var result = await _service.CreateAsync(dto);
        if (!result.Success || result.Data == null)
        {
            return BadRequest(result);
        }

        var newUser = result.Data;

        return CreatedAtAction(nameof(GetById), new { id = newUser.UserUuid }, newUser);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid guid, [FromBody]UserUpdateDto dto)
    {

        var result = await _service.UpdateAsync(guid, dto);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid guid)
    {
        var result = await _service.DeleteAsync(guid);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }
}