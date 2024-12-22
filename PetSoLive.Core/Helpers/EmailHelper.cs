using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Helpers
{
    public class EmailHelper
    {
        public string GenerateAdoptionRequestEmailBody(User user, Pet pet, AdoptionRequest adoptionRequest)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        background-color: #f6f6f6;
                        margin: 0;
                        padding: 0;
                        color: #333;
                    }}
                    .container {{
                        width: 100%;
                        max-width: 600px;
                        margin: 30px auto;
                        padding: 20px;
                        background-color: #ffffff;
                        border-radius: 8px;
                        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
                    }}
                    h2 {{
                        color: #2C3E50;
                        font-size: 28px;
                        margin-bottom: 20px;
                    }}
                    h3 {{
                        color: #2980B9;
                        font-size: 22px;
                        margin-top: 20px;
                    }}
                    p {{
                        font-size: 16px;
                        line-height: 1.6;
                        margin-bottom: 10px;
                    }}
                    ul {{
                        padding-left: 20px;
                    }}
                    li {{
                        font-size: 16px;
                        margin-bottom: 8px;
                    }}
                    .highlight {{
                        font-weight: bold;
                        color: #2980B9;
                    }}
                    .footer {{
                        margin-top: 30px;
                        text-align: center;
                        font-size: 14px;
                        color: #7f8c8d;
                    }}
                    .button {{
                        display: inline-block;
                        background-color: #2980B9;
                        color: white;
                        padding: 10px 20px;
                        font-size: 16px;
                        border-radius: 5px;
                        text-decoration: none;
                        margin-top: 20px;
                    }}
                    .button:hover {{
                        background-color: #1C4F76;
                    }}
                    .divider {{
                        border-top: 1px solid #eee;
                        margin: 20px 0;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>New Adoption Request for Your Pet: {pet.Name}</h2>
                    <p><span class='highlight'>Requested by:</span> {user.Username}</p>
                    <p><span class='highlight'>Message from the adopter:</span> {adoptionRequest.Message}</p>
                    <p><span class='highlight'>Status:</span> {adoptionRequest.Status}</p>

                    <div class='divider'></div>

                    <h3>Pet Details:</h3>
                    <ul>
                        <li><span class='highlight'>Name:</span> {pet.Name}</li>
                        <li><span class='highlight'>Species:</span> {pet.Species}</li>
                        <li><span class='highlight'>Breed:</span> {pet.Breed}</li>
                        <li><span class='highlight'>Age:</span> {pet.Age} years old</li>
                        <li><span class='highlight'>Gender:</span> {pet.Gender}</li>
                        <li><span class='highlight'>Weight:</span> {pet.Weight} kg</li>
                        <li><span class='highlight'>Color:</span> {pet.Color}</li>
                        <li><span class='highlight'>Vaccination Status:</span> {pet.VaccinationStatus}</li>
                        <li><span class='highlight'>Microchip ID:</span> {pet.MicrochipId}</li>
                        <li><span class='highlight'>Is Neutered:</span> {(pet.IsNeutered.HasValue ? (pet.IsNeutered.Value ? "Yes" : "No") : "Not specified")}</li>
                    </ul>

                    <div class='divider'></div>

                    <h3>User Details:</h3>
                    <ul>
                        <li><span class='highlight'>Name:</span> {user.Username}</li>
                        <li><span class='highlight'>Email:</span> {user.Email}</li>
                        <li><span class='highlight'>Phone:</span> {user.PhoneNumber}</li>
                        <li><span class='highlight'>Address:</span> {user.Address}</li>
                        <li><span class='highlight'>Date of Birth:</span> {user.DateOfBirth.ToString("yyyy-MM-dd")}</li>
                        <li><span class='highlight'>Account Created:</span> {user.CreatedDate.ToString("yyyy-MM-dd")}</li>
                    </ul>

                    <div class='divider'></div>

                    <p>If you wish to review or respond to this adoption request, please log into your account.</p>
                    <a href='#' class='button'>Review Adoption Request</a>

                    <div class='footer'>
                        <p>Best regards,</p>
                        <p>The PetSoLive Team</p>
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}
