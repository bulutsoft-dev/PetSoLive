using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Petsolive.API.DTOs;
using PetSoLive.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PetSoLive.Core.Helpers;

namespace Petsolive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;

    public PetController(IServiceManager serviceManager, IMapper mapper)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PetDto>>> GetAll()
    {
        var pets = await _serviceManager.PetService.GetAllPetsAsync();
        var petDtos = _mapper.Map<IEnumerable<PetDto>>(pets);
        return Ok(petDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PetDto>> GetById(int id)
    {
        var pet = await _serviceManager.PetService.GetPetByIdAsync(id);
        if (pet == null) return NotFound();
        return Ok(_mapper.Map<PetDto>(pet));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PetDto>> Create([FromBody] PetDto petDto)
    {
        petDto.Id = 0; // Ensure Id is not set
        var pet = _mapper.Map<Pet>(petDto);
        // DateOfBirth UTC olarak işaretle
        if (pet.DateOfBirth.Kind != DateTimeKind.Utc)
            pet.DateOfBirth = DateTime.SpecifyKind(pet.DateOfBirth, DateTimeKind.Utc);
        await _serviceManager.PetService.CreatePetAsync(pet);

        // JWT'den userId al
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized("Kullanıcı kimliği bulunamadı.");
        int userId = int.Parse(userIdClaim.Value);

        // PetOwner kaydı oluştur
        var ownershipDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        var petOwner = new PetOwner
        {
            PetId = pet.Id,
            UserId = userId,
            OwnershipDate = ownershipDate
        };
        await _serviceManager.PetService.AssignPetOwnerAsync(petOwner);

        // --- EMAIL ---
        var user = await _serviceManager.UserService.GetUserByIdAsync(userId);
        var emailHelper = new EmailHelper();
        if (user != null)
        {
            var body = emailHelper.GeneratePetCreationEmailBody(user, pet);
            await _serviceManager.EmailService.SendEmailAsync(user.Email, "Your Pet Has Been Created", body);
        }
        // --- EMAIL END ---

        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, _mapper.Map<PetDto>(pet));
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PetDto petDto)
    {
        if (petDto == null)
            return BadRequest("PetDto cannot be null.");

        var pet = _mapper.Map<Pet>(petDto);
        // DateOfBirth UTC olarak işaretle
        if (pet.DateOfBirth.Kind != DateTimeKind.Utc)
            pet.DateOfBirth = DateTime.SpecifyKind(pet.DateOfBirth, DateTimeKind.Utc);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized("Kullanıcı kimliği bulunamadı.");

        int userId = int.Parse(userIdClaim.Value);

        // Petin owner'ı var mı kontrol et
        var hasOwner = await _serviceManager.PetService.IsUserOwnerOfPetAsync(id, userId);
        if (!hasOwner)
            return Forbid("Bu petin sahibi değilsiniz veya petin sahibi yok.");

        await _serviceManager.PetService.UpdatePetAsync(id, pet, userId);

        // --- EMAIL ---
        // Bu pet için başvuru yapan kullanıcılara güncelleme e-postası gönder
        var adoptionRequests = await _serviceManager.AdoptionRequestService.GetAdoptionRequestsByPetIdAsync(id);
        var emailHelper = new EmailHelper();
        foreach (var request in adoptionRequests)
        {
            if (request.User != null)
            {
                var body = emailHelper.GeneratePetUpdateEmailBody(request.User, pet);
                await _serviceManager.EmailService.SendEmailAsync(request.User.Email, "Pet Information Updated", body);
            }
        }
        // --- EMAIL END ---

        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized("Kullanıcı kimliği bulunamadı.");

        int userId = int.Parse(userIdClaim.Value);

        // Petin owner'ı var mı kontrol et
        var hasOwner = await _serviceManager.PetService.IsUserOwnerOfPetAsync(id, userId);
        if (!hasOwner)
            return StatusCode(403, "Bu petin sahibi değilsiniz veya petin sahibi yok.");

        // --- EMAIL ---
        // Bu pet için başvuru yapan kullanıcılara silinme e-postası gönder
        var adoptionRequests = await _serviceManager.AdoptionRequestService.GetAdoptionRequestsByPetIdAsync(id);
        var pet = await _serviceManager.PetService.GetPetByIdAsync(id);
        var emailHelper = new EmailHelper();
        foreach (var request in adoptionRequests)
        {
            if (request.User != null)
            {
                var body = emailHelper.GeneratePetDeletionEmailBody(request.User, pet);
                await _serviceManager.EmailService.SendEmailAsync(request.User.Email, "Pet Deleted", body);
            }
        }
        // --- EMAIL END ---

        // Sadece peti sil, owner kaydını service veya cascade ile sil
        await _serviceManager.PetService.DeletePetAsync(id, userId);

        return NoContent();
    }
}