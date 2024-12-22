using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using Microsoft.AspNetCore.Identity;

public class AdoptionController : Controller
{
    private readonly IAdoptionService _adoptionService;
    private readonly IPetService _petService;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly IPetOwnerService _petOwnerService;

    public AdoptionController(IAdoptionService adoptionService, IPetService petService, IUserService userService, IEmailService emailService, IPetOwnerService petOwnerService)
    {
        _adoptionService = adoptionService;
        _petService = petService;
        _userService = userService;
        _emailService = emailService;
        _petOwnerService = petOwnerService;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Fetch all pets available for adoption
        var pets = await _petService.GetAllPetsAsync();

        return View(pets);  // Pass the list of pets to the view
    }
  // GET: /Adoption/Adopt/{id}
  public async Task<IActionResult> Adopt(int petId)
  {
      var username = HttpContext.Session.GetString("Username");
      if (username == null)
      {
          return RedirectToAction("Login", "Account");
      }

      // Kullanıcıyı çek
      var user = await _userService.GetUserByUsernameAsync(username);
      if (user == null)
      {
          return BadRequest("User not found.");
      }

      // Pet bilgisi
      var pet = await _petService.GetPetByIdAsync(petId);
      if (pet == null)
      {
          return NotFound();
      }

      // ViewData ile pet bilgisi gönder
      ViewData["PetName"] = pet.Name;
      ViewData["PetId"] = pet.Id;

      return View(user); // Model olarak kullanıcıyı gönder
  }


        // POST: /Adoption/Adopt
        [HttpPost]
        public async Task<IActionResult> Adopt(int petId, string name, string email, string phone, string message)
        {
            // Check if user session exists
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if the user is not logged in
            }

            // Fetch the pet details
            var pet = await _petService.GetPetByIdAsync(petId);
            if (pet == null)
            {
                return NotFound();
            }

            // Get the logged-in user from session
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // Create adoption request
            var adoptionRequest = new AdoptionRequest
            {
                PetId = petId,
                Message = message,
                Status = AdoptionStatus.Pending,
                RequestDate = DateTime.Now,
                UserId = user.Id,  // Automatically set user from the session
                User = user         // Set the navigation property for the user
            };

            // Save the adoption request
            await _adoptionService.CreateAdoptionRequestAsync(adoptionRequest);

            // Send notification to the pet owner
            await SendAdoptionRequestNotificationAsync(adoptionRequest);

            // Redirect to the pet details page after adoption request
            return RedirectToAction("Details", "Pet", new { id = petId });
        }

// This method sends an email to the pet owner regarding the new adoption request
// This method sends an email to the pet owner regarding the new adoption request
public async Task SendAdoptionRequestNotificationAsync(AdoptionRequest adoptionRequest)
{
    var petOwner = await _petOwnerService.GetPetOwnerByPetIdAsync(adoptionRequest.PetId);
    var petOwnerUser = await _userService.GetUserByIdAsync(petOwner.UserId);

    // Fetching detailed user and pet information
    var user = adoptionRequest.User;
    var pet = adoptionRequest.Pet;

    var subject = "New Adoption Request for Your Pet";
    var body = $@"
    <h2>New Adoption Request for Your Pet: {pet.Name}</h2>
    <p><strong>Requested by:</strong> {user.Username}</p>
    <p><strong>Message from the adopter:</strong> {adoptionRequest.Message}</p>
    <p><strong>Status:</strong> {adoptionRequest.Status}</p>

    <h3>Pet Details:</h3>
    <ul>
        <li><strong>Name:</strong> {pet.Name}</li>
        <li><strong>Species:</strong> {pet.Species}</li>
        <li><strong>Breed:</strong> {pet.Breed}</li>
        <li><strong>Age:</strong> {pet.Age} years old</li>
        <li><strong>Gender:</strong> {pet.Gender}</li>
        <li><strong>Weight:</strong> {pet.Weight} kg</li>
        <li><strong>Color:</strong> {pet.Color}</li>
        <li><strong>Vaccination Status:</strong> {pet.VaccinationStatus}</li>
        <li><strong>Microchip ID:</strong> {pet.MicrochipId}</li>
        <li><strong>Is Neutered:</strong> {(pet.IsNeutered.HasValue ? (pet.IsNeutered.Value ? "Yes" : "No") : "Not specified")}</li>
    </ul>

    <h3>User Details:</h3>
    <ul>
        <li><strong>Name:</strong> {user.Username}</li>
        <li><strong>Email:</strong> {user.Email}</li>
        <li><strong>Phone:</strong> {user.PhoneNumber}</li>
        <li><strong>Address:</strong> {user.Address}</li>
        <li><strong>Date of Birth:</strong> {user.DateOfBirth.ToString("yyyy-MM-dd")}</li>
        <li><strong>Account Created:</strong> {user.CreatedDate.ToString("yyyy-MM-dd")}</li>
    </ul>

    <p>Best regards,</p>
    <p>The PetSoLive Team</p>";

    // Sending the email
    await _emailService.SendEmailAsync(petOwnerUser.Email, subject, body);
}


}
