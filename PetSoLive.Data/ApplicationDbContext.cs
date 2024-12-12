// 3. Data Access Layer (/PetSoLive.Data)
using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;

namespace PetSoLive.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Adoption> Adoptions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=PetSoLiveDB;User Id=sa;Password=pass;Encrypt=False;TrustServerCertificate=True");
        }
    }

}