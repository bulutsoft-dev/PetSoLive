using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetUsersByLocationAsync(string city, string district);
    }
}