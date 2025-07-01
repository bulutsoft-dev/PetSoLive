using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Petsolive.API.DTOs;
using PetSoLive.Core.Entities;

namespace Petsolive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelpRequestController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;

    public HelpRequestController(IServiceManager serviceManager, IMapper mapper)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HelpRequestDto>>> GetAll()
    {
        var helpRequests = await _serviceManager.HelpRequestService.GetHelpRequestsAsync();
        return Ok(_mapper.Map<IEnumerable<HelpRequestDto>>(helpRequests));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HelpRequestDto>> GetById(int id)
    {
        var helpRequest = await _serviceManager.HelpRequestService.GetHelpRequestByIdAsync(id);
        if (helpRequest == null) return NotFound();
        return Ok(_mapper.Map<HelpRequestDto>(helpRequest));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] HelpRequestDto dto)
    {
        var entity = _mapper.Map<HelpRequest>(dto);
        await _serviceManager.HelpRequestService.CreateHelpRequestAsync(entity);
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] HelpRequestDto dto)
    {
        var entity = _mapper.Map<HelpRequest>(dto);
        entity.Id = id;
        await _serviceManager.HelpRequestService.UpdateHelpRequestAsync(entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _serviceManager.HelpRequestService.DeleteHelpRequestAsync(id);
        return NoContent();
    }
}