using PetSoLive.Core.Interfaces;

namespace PetSoLive.Business.Services
{
    public class ServiceManager : IServiceManager
    {
        public ServiceManager(
            IAdminService adminService,
            IAdoptionRequestService adoptionRequestService,
            IAdoptionService adoptionService,
            ICommentService commentService,
            IEmailService emailService,
            IHelpRequestService helpRequestService,
            ILostPetAdService lostPetAdService,
            IPetOwnerService petOwnerService,
            IPetService petService,
            IUserService userService,
            IVeterinarianService veterinarianService)
        {
            AdminService = adminService;
            AdoptionRequestService = adoptionRequestService;
            AdoptionService = adoptionService;
            CommentService = commentService;
            EmailService = emailService;
            HelpRequestService = helpRequestService;
            LostPetAdService = lostPetAdService;
            PetOwnerService = petOwnerService;
            PetService = petService;
            UserService = userService;
            VeterinarianService = veterinarianService;
        }

        public IAdminService AdminService { get; }
        public IAdoptionRequestService AdoptionRequestService { get; }
        public IAdoptionService AdoptionService { get; }
        public ICommentService CommentService { get; }
        public IEmailService EmailService { get; }
        public IHelpRequestService HelpRequestService { get; }
        public ILostPetAdService LostPetAdService { get; }
        public IPetOwnerService PetOwnerService { get; }
        public IPetService PetService { get; }
        public IUserService UserService { get; }
        public IVeterinarianService VeterinarianService { get; }
    }
}