using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetSoLive.Core.Enums;

namespace PetSoLive.Core.Services
{
    public class VeterinarianService : IVeterinarianService
    {
        private readonly IVeterinarianRepository _veterinarianRepository;

        public VeterinarianService(IVeterinarianRepository veterinarianRepository)
        {
            _veterinarianRepository = veterinarianRepository;
        }

        public async Task<Veterinarian> RegisterVeterinarianAsync(int userId, string qualifications, string clinicAddress, string clinicPhoneNumber)
        {
            var veterinarian = new Veterinarian
            {
                UserId = userId,
                Qualifications = qualifications,
                ClinicAddress = clinicAddress,
                ClinicPhoneNumber = clinicPhoneNumber,
                AppliedDate = DateTime.UtcNow,
                Status = VeterinarianStatus.Pending  // Default to Pending status
            };

            return await _veterinarianRepository.CreateAsync(veterinarian);
        }

        public async Task<Veterinarian> GetByUserIdAsync(int userId)
        {
            return await _veterinarianRepository.GetByUserIdAsync(userId);
        }

        public async Task<Veterinarian> GetByIdAsync(int id)  // Add this method
        {
            return await _veterinarianRepository.GetByIdAsync(id);
        }

        public async Task ApproveVeterinarianAsync(int veterinarianId)
        {
            var veterinarian = await _veterinarianRepository.GetByIdAsync(veterinarianId);
            if (veterinarian == null)
            {
                throw new KeyNotFoundException("Veterinarian not found");
            }

            veterinarian.Status = VeterinarianStatus.Approved;
            await _veterinarianRepository.UpdateAsync(veterinarian);
        }

        public async Task RejectVeterinarianAsync(int veterinarianId)
        {
            var veterinarian = await _veterinarianRepository.GetByIdAsync(veterinarianId);
            if (veterinarian == null)
            {
                throw new KeyNotFoundException("Veterinarian not found");
            }

            veterinarian.Status = VeterinarianStatus.Rejected;
            await _veterinarianRepository.UpdateAsync(veterinarian);
        }

        public async Task<IEnumerable<Veterinarian>> GetAllVeterinariansAsync()
        {
            return await _veterinarianRepository.GetAllAsync();
        }
    }
}
