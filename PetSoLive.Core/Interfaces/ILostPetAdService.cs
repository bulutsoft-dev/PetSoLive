using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Core.Interfaces
{
    public interface ILostPetAdService
    {
        Task CreateLostPetAdAsync(LostPetAd lostPetAd, string city, string district);  // Kayıp ilanı oluşturma
        Task<IEnumerable<LostPetAd>> GetAllLostPetAdsAsync();                         // Tüm kayıp ilanlarını alma
    }
}