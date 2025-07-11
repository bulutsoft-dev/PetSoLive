using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Petsolive.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Petsolive.API;
using PetSoLive.Data;
using PetSoLive.Core.Entities;
using PetSoLive.Core;

namespace Petsolive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdoptionRequestController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;

    public AdoptionRequestController(IServiceManager serviceManager, IMapper mapper, ApplicationDbContext context)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AdoptionRequestDto>> GetById(int id)
    {
        var req = await _serviceManager.AdoptionRequestService.GetAdoptionRequestByIdAsync(id);
        if (req == null) return NotFound();
        return Ok(_mapper.Map<AdoptionRequestDto>(req));
    }

    [HttpGet("pet/{petId}")]
    public async Task<ActionResult<IEnumerable<AdoptionRequestDto>>> GetByPetId(int petId)
    {
        var requests = await _serviceManager.AdoptionRequestService.GetAdoptionRequestsByPetIdAsync(petId);
        if (requests == null || !requests.Any())
            return NotFound();

        var dtos = requests.Select(r => _mapper.Map<AdoptionRequestDto>(r));
        return Ok(dtos);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] AdoptionRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        try
        {
            var entity = _mapper.Map<PetSoLive.Core.Entities.AdoptionRequest>(dto);
            entity.Id = 0; // Yeni kayıt için
            entity.Status = PetSoLive.Core.Enums.AdoptionStatus.Pending;
            // RequestDate UTC olarak işaretle
            if (entity.RequestDate.Kind != DateTimeKind.Utc)
                entity.RequestDate = DateTime.SpecifyKind(entity.RequestDate, DateTimeKind.Utc);
            if (entity.RequestDate == default)
                entity.RequestDate = DateTime.UtcNow;

            await _serviceManager.AdoptionService.CreateAdoptionRequestAsync(entity);

            // --- EMAIL ---
            // Pet sahibi ve başvuran kullanıcıya e-posta gönder
            var petOwner = await _serviceManager.PetOwnerService.GetPetOwnerByPetIdAsync(entity.PetId);
            var pet = entity.Pet ?? await _serviceManager.PetService.GetPetByIdAsync(entity.PetId);
            var user = entity.User ?? await _serviceManager.UserService.GetUserByIdAsync(entity.UserId);
            var emailHelper = new EmailHelper();
            // Pet sahibine e-posta
            if (petOwner?.User != null)
            {
                var ownerBody = emailHelper.GenerateAdoptionRequestEmailBody(user, pet, entity);
                await _serviceManager.EmailService.SendEmailAsync(petOwner.User.Email, "New Adoption Request for Your Pet", ownerBody);
            }
            // Başvuran kullanıcıya e-posta
            if (user != null)
            {
                var userBody = emailHelper.GenerateAdoptionRequestConfirmationEmailBody(user, pet);
                await _serviceManager.EmailService.SendEmailAsync(user.Email, "Adoption Request Submitted Successfully", userBody);
            }
            // --- EMAIL END ---

            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("{id}/approve")]
    [Authorize]
    public async Task<IActionResult> Approve(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var request = await _context.AdoptionRequests
                .Include(r => r.User)
                .Include(r => r.Pet)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            if (request.Status == PetSoLive.Core.Enums.AdoptionStatus.Approved)
                return BadRequest(new { error = "Request is already approved." });

            if (request.User == null || request.Pet == null)
                return BadRequest(new { error = "AdoptionRequest'in User veya Pet ilişkisi eksik (null). Lütfen başvurunun ilişkili kullanıcı ve pet ile birlikte geldiğinden emin olun." });

            request.Status = PetSoLive.Core.Enums.AdoptionStatus.Approved;
            if (request.RequestDate.Kind != DateTimeKind.Utc)
                request.RequestDate = DateTime.SpecifyKind(request.RequestDate, DateTimeKind.Utc);
            _context.AdoptionRequests.Update(request);

            // Diğer başvuruları reddet
            var allRequests = await _context.AdoptionRequests
                .Include(r => r.User)
                .Where(r => r.PetId == request.PetId && r.Id != id && r.Status == PetSoLive.Core.Enums.AdoptionStatus.Pending)
                .ToListAsync();
            foreach (var r in allRequests)
            {
                r.Status = PetSoLive.Core.Enums.AdoptionStatus.Rejected;
                if (r.RequestDate.Kind != DateTimeKind.Utc)
                    r.RequestDate = DateTime.SpecifyKind(r.RequestDate, DateTimeKind.Utc);
                _context.AdoptionRequests.Update(r);
            }

            // Adoption tablosuna kayıt eklemeden önce UTC olarak işaretle
            var adoptionDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            var adoption = new PetSoLive.Core.Entities.Adoption
            {
                PetId = request.PetId,
                UserId = request.UserId,
                AdoptionDate = adoptionDate,
                Status = PetSoLive.Core.Enums.AdoptionStatus.Approved
            };
            if (adoption.AdoptionDate.Kind != DateTimeKind.Utc)
                adoption.AdoptionDate = DateTime.SpecifyKind(adoption.AdoptionDate, DateTimeKind.Utc);
            _context.Adoptions.Add(adoption);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // --- EMAIL ---
            var emailHelper = new EmailHelper();
            var pet = request.Pet;
            // Onaylanan kullanıcıya e-posta
            if (request.User != null && pet != null)
            {
                var approvedBody = emailHelper.GenerateAdoptionConfirmationEmailBody(request.User, pet);
                await _serviceManager.EmailService.SendEmailAsync(request.User.Email, "Your Adoption Request Has Been Approved", approvedBody);
            }
            // Diğer başvuranlara red e-postası
            foreach (var r in allRequests)
            {
                if (r.User != null && pet != null)
                {
                    var rejectedBody = emailHelper.GenerateRejectionEmailBody(r.User, pet);
                    await _serviceManager.EmailService.SendEmailAsync(r.User.Email, "Adoption Request Rejected", rejectedBody);
                }
            }
            // --- EMAIL END ---

            return Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("{id}/reject")]
    [Authorize]
    public async Task<IActionResult> Reject(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var request = await _context.AdoptionRequests
                .Include(r => r.User)
                .Include(r => r.Pet)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            if (request.Status == PetSoLive.Core.Enums.AdoptionStatus.Rejected)
                return BadRequest(new { error = "Request is already rejected." });

            if (request.User == null || request.Pet == null)
                return BadRequest(new { error = "AdoptionRequest'in User veya Pet ilişkisi eksik (null). Lütfen başvurunun ilişkili kullanıcı ve pet ile birlikte geldiğinden emin olun." });

            request.Status = PetSoLive.Core.Enums.AdoptionStatus.Rejected;
            if (request.RequestDate.Kind != DateTimeKind.Utc)
                request.RequestDate = DateTime.SpecifyKind(request.RequestDate, DateTimeKind.Utc);
            _context.AdoptionRequests.Update(request);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // --- EMAIL ---
            var emailHelper = new EmailHelper();
            var pet = request.Pet;
            if (request.User != null && pet != null)
            {
                var rejectedBody = emailHelper.GenerateRejectionEmailBody(request.User, pet);
                await _serviceManager.EmailService.SendEmailAsync(request.User.Email, "Adoption Request Rejected", rejectedBody);
            }
            // --- EMAIL END ---

            return Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { error = ex.Message });
        }
    }
}