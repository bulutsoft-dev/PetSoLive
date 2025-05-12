using PetSoLive.Core.Interfaces;

public interface IServiceManager
{
    IAdminService AdminService { get; }
    IAdoptionRequestService AdoptionRequestService { get; }
    IAdoptionService AdoptionService { get; }
    ICommentService CommentService { get; }
    IEmailService EmailService { get; }
    IHelpRequestService HelpRequestService { get; }
    ILostPetAdService LostPetAdService { get; }
    IPetOwnerService PetOwnerService { get; }
    IPetService PetService { get; }
    IUserService UserService { get; }
    IVeterinarianService VeterinarianService { get; }
}