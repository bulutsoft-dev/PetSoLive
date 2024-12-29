using PetSoLive.Core.Entities;

public class EmailHelper
{
    private const string CssLink = "<link rel='stylesheet' type='text/css' href='https://yourdomain.com/path/to/email.css'>";

    public string GenerateAdoptionRequestEmailBody(User user, Pet pet, AdoptionRequest adoptionRequest)
    {
        return $@"
        <html>
        <head>
            {CssLink}
        </head>
        <body>
            <div class='email-container'>
                <h2 class='header'>New Adoption Request for Your Pet: {pet.Name}</h2>
                <p class='info'><strong>Requested by:</strong> {user.Username}</p>
                <p class='info'><strong>Message from the adopter:</strong> {adoptionRequest.Message}</p>
                <p class='status'><strong>Status:</strong> {adoptionRequest.Status}</p>

                <div class='divider'></div>

                <h3 class='section-header'>Pet Details:</h3>
                <ul class='details-list'>
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

                <div class='divider'></div>

                <h3 class='section-header'>User Details:</h3>
                <ul class='details-list'>
                    <li><strong>Name:</strong> {user.Username}</li>
                    <li><strong>Email:</strong> {user.Email}</li>
                    <li><strong>Phone:</strong> {user.PhoneNumber}</li>
                    <li><strong>Address:</strong> {user.Address}</li>
                    <li><strong>Date of Birth:</strong> {user.DateOfBirth.ToString("yyyy-MM-dd")}</li>
                    <li><strong>Account Created:</strong> {user.CreatedDate.ToString("yyyy-MM-dd")}</li>
                </ul>

                <div class='divider'></div>

                <p>If you wish to review or respond to this adoption request, please log into your account.</p>
                <a href='#' class='btn-primary'>Review Adoption Request</a>

                <div class='footer'>
                    <p>Best regards,</p>
                    <p>The PetSoLive Team</p>
                </div>
            </div>
        </body>
        </html>";
    }

    public string GenerateAdoptionRequestConfirmationEmailBody(User user, Pet pet)
    {
        return $@"
        <html>
        <head>
            {CssLink}
        </head>
        <body>
            <div class='email-container'>
                <h2 class='header'>Thank you for your Adoption Request!</h2>
                <p>Dear {user.Username},</p>
                <p>Thank you for your adoption request for {pet.Name}. Your request has been successfully submitted and is currently under review.</p>
                <p>We will notify you once your request has been processed.</p>

                <div class='footer'>
                    <p>Best regards,</p>
                    <p>The PetSoLive Team</p>
                </div>
            </div>
        </body>
        </html>";
    }

    public string GenerateRejectionEmailBody(User user, Pet pet)
    {
        return $@"
        <html>
        <head>
            {CssLink}
        </head>
        <body>
            <div class='email-container'>
                <h2 class='header'>Your Adoption Request for {pet.Name} Has Been Rejected</h2>
                <p>Dear {user.Username},</p>
                <p>We regret to inform you that your adoption request for {pet.Name} has been rejected. Unfortunately, another user’s request for this pet has been approved.</p>

                <div class='footer'>
                    <p>Best regards,</p>
                    <p>The PetSoLive Team</p>
                </div>
            </div>
        </body>
        </html>";
    }

    public string GenerateAdoptionConfirmationEmailBody(User user, Pet pet)
    {
        return $@"
        <html>
        <head>
            {CssLink}
        </head>
        <body>
            <div class='email-container'>
                <h2 class='header'>Your Adoption Request for {pet.Name} Has Been Approved</h2>
                <p>Dear {user.Username},</p>
                <p>We are pleased to inform you that your adoption request for <span class='highlight'>{pet.Name}</span> has been approved!</p>
                <p>Thank you for choosing to adopt, and we hope you and {pet.Name} will have a wonderful time together.</p>

                <div class='footer'>
                    <p>Best regards,</p>
                    <p>The PetSoLive Team</p>
                </div>
            </div>
        </body>
        </html>";
    }

    public string GeneratePetDeletionEmailBody(User user, Pet pet)
    {
        return $@"
        <html>
        <head>
            {CssLink}
        </head>
        <body>
            <div class='email-container'>
                <h2 class='header'>Pet Removed: {pet.Name}</h2>
                <p>Dear {user.Username},</p>
                <p>We regret to inform you that the pet <span class='highlight'>{pet.Name}</span> you were interested in has been removed from our platform. It is no longer available for adoption.</p>
                <p>We apologize for any inconvenience this may have caused. Please feel free to explore other available pets.</p>

                <div class='footer'>
                    <p>Best regards,</p>
                    <p>The PetSoLive Team</p>
                </div>
            </div>
        </body>
        </html>";
    }

    public string GeneratePetUpdateEmailBody(User user, Pet pet)
    {
        return $@"
        <html>
        <head>
            {CssLink}
        </head>
        <body>
            <div class='email-container'>
                <h2 class='header'>Pet Information Updated: {pet.Name}</h2>
                <p>Dear {user.Username},</p>
                <p>The details of the pet you were interested in, <span class='highlight'>{pet.Name}</span>, have been updated. Please review the updated information.</p>
                <p>Here are the updated details:</p>
                <ul class='details-list'>
                    <li><strong>Name:</strong> {pet.Name}</li>
                    <li><strong>Breed:</strong> {pet.Breed}</li>
                    <li><strong>Age:</strong> {pet.Age} years old</li>
                </ul>
                <p>Please log into your account to view the full details.</p>

                <div class='footer'>
                    <p>Best regards,</p>
                    <p>The PetSoLive Team</p>
                </div>
            </div>
        </body>
        </html>";
    }

    public string GeneratePetCreationEmailBody(User user, Pet pet)
    {
        return $@"
        <html>
        <head>
            {CssLink}
        </head>
        <body>
            <div class='email-container'>
                <h2 class='header'>Your Pet {pet.Name} Has Been Successfully Created</h2>
                <p>Dear {user.Username},</p>
                <p>Thank you for creating a profile for your pet, <span class='highlight'>{pet.Name}</span>. The pet has been successfully added to our system.</p>
                <p>You can now view and manage the pet's details from your profile.</p>

                <div class='footer'>
                    <p>Best regards,</p>
                    <p>The PetSoLive Team</p>
                </div>
            </div>
        </body>
        </html>";
    }
    
    
    public string GenerateVeterinarianNotificationEmailBody(HelpRequest helpRequest, User requester)
    {
        return $@"
        <html>
        <head>
            {CssLink}
        </head>
        <body>
            <div class='email-container'>
                <h2 class='header'>New Help Request for an Animal in Need!</h2>
                <p>Dear Veterinarian,</p>
                <p>A new help request has been created for an animal requiring immediate attention:</p>
                <ul class='details-list'>
                    <li><strong>Description:</strong> {helpRequest.Description}</li>
                    <li><strong>Emergency Level:</strong> {helpRequest.EmergencyLevel}</li>
                    <li><strong>Created At:</strong> {helpRequest.CreatedAt.ToString("yyyy-MM-dd HH:mm")}</li>
                    <li><strong>Requested By:</strong> {requester.Username} ({requester.Email})</li>
                </ul>

                <p>Please log in to the system to review this request in detail and provide assistance.</p>
                <a href='https://yourdomain.com/HelpRequest/Details/{helpRequest.Id}' class='btn-primary'>View Help Request</a>

                <div class='footer'>
                    <p>Best regards,</p>
                    <p>The PetSoLive Team</p>
                </div>
            </div>
        </body>
        </html>";
    }
    
       // Create HelpRequest Email Body
    public string GenerateCreateHelpRequestEmailBody(HelpRequest helpRequest, User requester)
    {
        return $@"
        <html>
        <head>
            {CssLink}
        </head>
        <body>
            <div class='email-container'>
                <h2 class='header'>New Help Request Created for an Animal in Need!</h2>
                <p>Dear Veterinarian,</p>
                <p>A new help request has been created for an animal requiring immediate attention:</p>
                <ul class='details-list'>
                    <li><strong>Description:</strong> {helpRequest.Description}</li>
                    <li><strong>Emergency Level:</strong> {helpRequest.EmergencyLevel}</li>
                    <li><strong>Created At:</strong> {helpRequest.CreatedAt.ToString("yyyy-MM-dd HH:mm")}</li>
                    <li><strong>Requested By:</strong> {requester.Username} ({requester.Email})</li>
                </ul>

                <p>Please log in to the system to review this request in detail and provide assistance.</p>
                <a href='https://yourdomain.com/HelpRequest/Details/{helpRequest.Id}' class='btn-primary'>View Help Request</a>

                <div class='footer'>
                    <p>Best regards,</p>
                    <p>The PetSoLive Team</p>
                </div>
            </div>
        </body>
        </html>";
    }

    // Edit HelpRequest Email Body
    public string GenerateEditHelpRequestEmailBody(HelpRequest helpRequest, User requester)
    {
        return $@"
        <html>
        <head>
            {CssLink}
        </head>
        <body>
            <div class='email-container'>
                <h2 class='header'>Help Request Updated for an Animal in Need!</h2>
                <p>Dear Veterinarian,</p>
                <p>The following help request has been updated:</p>
                <ul class='details-list'>
                    <li><strong>Description:</strong> {helpRequest.Description}</li>
                    <li><strong>Emergency Level:</strong> {helpRequest.EmergencyLevel}</li>
                    <li><strong>Updated At:</strong> {helpRequest.CreatedAt.ToString("yyyy-MM-dd HH:mm")}</li>
                    <li><strong>Requested By:</strong> {requester.Username} ({requester.Email})</li>
                </ul>

                <p>Please log in to the system to review the updated request in detail.</p>
                <a href='https://yourdomain.com/HelpRequest/Details/{helpRequest.Id}' class='btn-primary'>View Updated Help Request</a>

                <div class='footer'>
                    <p>Best regards,</p>
                    <p>The PetSoLive Team</p>
                </div>
            </div>
        </body>
        </html>";
    }

    // Delete HelpRequest Email Body
    public string GenerateDeleteHelpRequestEmailBody(HelpRequest helpRequest, User requester)
    {
        return $@"
        <html>
        <head>
            {CssLink}
        </head>
        <body>
            <div class='email-container'>
                <h2 class='header'>Help Request Deleted for an Animal in Need!</h2>
                <p>Dear Veterinarian,</p>
                <p>The following help request has been deleted:</p>
                <ul class='details-list'>
                    <li><strong>Description:</strong> {helpRequest.Description}</li>
                    <li><strong>Emergency Level:</strong> {helpRequest.EmergencyLevel}</li>
                    <li><strong>Deleted At:</strong> {helpRequest.CreatedAt.ToString("yyyy-MM-dd HH:mm")}</li>
                    <li><strong>Requested By:</strong> {requester.Username} ({requester.Email})</li>
                </ul>

                <p>Please note that this request is no longer available for review.</p>

                <div class='footer'>
                    <p>Best regards,</p>
                    <p>The PetSoLive Team</p>
                </div>
            </div>
        </body>
        </html>";
    }
    // Yeni yorum için e-posta gövdesi oluşturma
    public string GenerateNewCommentEmailBody(HelpRequest helpRequest, Comment comment, User commenter)
    {
        return "<h2>New Comment on Help Request: Animal in Need!</h2>" +
               "<p><strong>Help Request Title:</strong> " + helpRequest.Title + "</p>" +
               "<p><strong>Description:</strong> " + helpRequest.Description + "</p>" +
               "<p><strong>Comment by:</strong> " + commenter.Username + "</p>" +
               "<p><strong>Comment Content:</strong> " + comment.Content + "</p>" +
               "<p><strong>Comment Date:</strong> " + comment.CreatedAt.ToString("dd MMM yyyy, hh:mm tt") + "</p>" +
               "<p><strong>Location:</strong> " + helpRequest.Location + "</p>" +
               "<br>" +
               "<p>Please check the help request and the comment left by the user.</p>" +
               "<p><a href='"+ "/HelpRequest/Details/" + helpRequest.Id + "'>Click here to view the help request</a></p>" +
               "<br>" +
               "<p>Thank you for your attention.</p>";
    }
}
