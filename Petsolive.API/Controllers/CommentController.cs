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
        if (!ModelState.IsValid)
            return BadRequest();
        var entity = _mapper.Map<Comment>(dto);
        // CreatedAt UTC olarak işaretle
        if (entity.CreatedAt.Kind != DateTimeKind.Utc)
            entity.CreatedAt = DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc);
        if (entity.CreatedAt == default)
            entity.CreatedAt = DateTime.UtcNow;
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

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Edit(int id, [FromBody] CommentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var existingComment = await _serviceManager.CommentService.GetCommentByIdAsync(id);
        if (existingComment == null)
            return NotFound();
        // Kullanıcı kontrolü (sadece kendi yorumunu düzenleyebilir)
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
        if (userIdClaim == null || existingComment.UserId.ToString() != userIdClaim.Value)
            return Unauthorized();
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            ModelState.AddModelError(nameof(dto.Content), "Content cannot be empty.");
            return BadRequest(ModelState);
        }
        existingComment.Content = dto.Content;
        existingComment.CreatedAt = DateTime.UtcNow;
        if (existingComment.CreatedAt.Kind != DateTimeKind.Utc)
            existingComment.CreatedAt = DateTime.SpecifyKind(existingComment.CreatedAt, DateTimeKind.Utc);
        // Diğer alanlar güncellenmek istenirse eklenebilir
        await _serviceManager.CommentService.UpdateCommentAsync(existingComment);
        return Ok(_mapper.Map<CommentDto>(existingComment));
    }
}