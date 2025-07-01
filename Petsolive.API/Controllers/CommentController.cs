using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Petsolive.API.DTOs;
using PetSoLive.Core.Entities;

namespace Petsolive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;

    public CommentController(IServiceManager serviceManager, IMapper mapper)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
    }

    [HttpGet("help-request/{helpRequestId}")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetByHelpRequestId(int helpRequestId)
    {
        var comments = await _serviceManager.CommentService.GetCommentsByHelpRequestIdAsync(helpRequestId);
        return Ok(_mapper.Map<IEnumerable<CommentDto>>(comments));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Add([FromBody] CommentDto dto)
    {
        var entity = _mapper.Map<Comment>(dto);
        await _serviceManager.CommentService.AddCommentAsync(entity);
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        await _serviceManager.CommentService.DeleteCommentAsync(id);
        return NoContent();
    }
}