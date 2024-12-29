using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Core.Interfaces
{
    public interface ILostPetAdRepository
    {
        // Yeni bir kayıp ilanı eklemek için metod
        Task CreateLostPetAdAsync(LostPetAd lostPetAd);

        // Tüm kayıp ilanlarını almak için metod
        Task<IEnumerable<LostPetAd>> GetAllAsync();

        // ID'ye göre bir kayıp ilanını almak için metod
        Task<LostPetAd> GetByIdAsync(int id);

        // Kayıp ilanını güncellemek için metod
        Task UpdateLostPetAdAsync(LostPetAd lostPetAd);

        // Kayıp ilanını silmek için metod
        Task DeleteLostPetAdAsync(LostPetAd lostPetAd);
    }
}