using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;

namespace PetSoLive.Business.Services
{
    public class AssistanceService : IAssistanceService
    {
        private readonly IRepository<Assistance> _assistanceRepository;

        public AssistanceService(IRepository<Assistance> assistanceRepository)
        {
            _assistanceRepository = assistanceRepository;
        }

        public async Task<IEnumerable<Assistance>> GetAllAssistancesAsync()
        {
            return await _assistanceRepository.GetAllAsync();
        }

        public async Task CreateAssistanceAsync(Assistance assistance)
        {
            await _assistanceRepository.AddAsync(assistance);
        }
    }
}