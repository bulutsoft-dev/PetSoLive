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
            IVeterinarianService veterinarianService,
            IAdoptionRequestRepository adoptionRequestRepository)
        {
            AdminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
            AdoptionRequestService = adoptionRequestService ?? throw new ArgumentNullException(nameof(adoptionRequestService));
            AdoptionService = adoptionService ?? throw new ArgumentNullException(nameof(adoptionService));
            CommentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
            EmailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            HelpRequestService = helpRequestService ?? throw new ArgumentNullException(nameof(helpRequestService));
            LostPetAdService = lostPetAdService ?? throw new ArgumentNullException(nameof(lostPetAdService));
            PetOwnerService = petOwnerService ?? throw new ArgumentNullException(nameof(petOwnerService));
            PetService = petService ?? throw new ArgumentNullException(nameof(petService));
            UserService = userService ?? throw new ArgumentNullException(nameof(userService));
            VeterinarianService = veterinarianService ?? throw new ArgumentNullException(nameof(veterinarianService));
            AdoptionRequestRepository = adoptionRequestRepository ?? throw new ArgumentNullException(nameof(adoptionRequestRepository));
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
        public IAdoptionRequestRepository AdoptionRequestRepository { get; }
    }
}