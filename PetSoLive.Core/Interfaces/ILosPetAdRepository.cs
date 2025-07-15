using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetSoLive.Core.DTOs;

namespace PetSoLive.Core.Interfaces
{
    public interface ILostPetAdRepository
    {
        // Yeni bir kayıp ilanı eklemek için metod
        Task CreateLostPetAdAsync(LostPetAd lostPetAd);

        // Tüm kayıp ilanlarını almak için metod
        Task<IEnumerable<LostPetAd>> GetAllAsync();

        // Filtreli kayıp ilanlarını almak için metod
        Task<IEnumerable<LostPetAd>> GetFilteredAsync(LostPetAdFilterDto filterDto);

        // ID'ye göre bir kayıp ilanını almak için metod
        Task<LostPetAd> GetByIdAsync(int id);

        // Kayıp ilanını güncellemek için metod
        Task UpdateLostPetAdAsync(LostPetAd lostPetAd);

        // Kayıp ilanını silmek için metod
        Task DeleteLostPetAdAsync(LostPetAd lostPetAd);
    }
}