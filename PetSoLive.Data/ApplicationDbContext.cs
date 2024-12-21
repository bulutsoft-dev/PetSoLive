using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;

namespace PetSoLive.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor accepting DbContextOptions
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Adoption> Adoptions { get; set; }
        public DbSet<Assistance> Assistances { get; set; }
        public DbSet<PetOwner> PetOwners { get; set; } // Add PetOwner table

        // Configure relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship between Pet and Adoption
            modelBuilder.Entity<Adoption>()
                .HasOne(a => a.Pet)
                .WithMany() // Pet can have multiple adoptions (adjust if needed)
                .HasForeignKey(a => a.PetId);

            modelBuilder.Entity<Adoption>()
                .HasOne(a => a.User)
                .WithMany() // User can adopt multiple pets (adjust if needed)
                .HasForeignKey(a => a.UserId);

            // Configure the many-to-many relationship between Pet and User through PetOwner
            modelBuilder.Entity<PetOwner>()
                .HasKey(po => new { po.PetId, po.UserId }); // Composite primary key

            modelBuilder.Entity<PetOwner>()
                .HasOne(po => po.Pet)
                .WithMany(p => p.PetOwners) // Pet can have multiple owners
                .HasForeignKey(po => po.PetId);

            modelBuilder.Entity<PetOwner>()
                .HasOne(po => po.User)
                .WithMany(u => u.PetOwners) // User can own multiple pets
                .HasForeignKey(po => po.UserId);
            
            
            // Configure Pet-User many-to-many relationship via PetOwner
            modelBuilder.Entity<PetOwner>()
                .HasKey(po => new { po.PetId, po.UserId });  // Composite key for PetOwner

            modelBuilder.Entity<PetOwner>()
                .HasOne(po => po.Pet)
                .WithMany(p => p.PetOwners)
                .HasForeignKey(po => po.PetId);

            modelBuilder.Entity<PetOwner>()
                .HasOne(po => po.User)
                .WithMany(u => u.PetOwners)
                .HasForeignKey(po => po.UserId);
        }
    }
}