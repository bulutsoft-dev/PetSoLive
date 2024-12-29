using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Core.Interfaces
{
    public interface ILostPetAdRepository
    {
        Task CreateLostPetAdAsync(LostPetAd lostPetAd);  // Kayıp ilanı oluşturma
        Task<IEnumerable<LostPetAd>> GetAllAsync();      // Bütün kayıp ilanlarını getirme
        Task<LostPetAd> GetByIdAsync(int id);            // Kayıp ilanını ID ile getirme (İhtiyaca göre eklenebilir)
    }
}