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

        // DbSets representing each table
        public DbSet<User> Users { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Adoption> Adoptions { get; set; }
        public DbSet<Assistance> Assistances { get; set; }
        public DbSet<PetOwner> PetOwners { get; set; }  // PetOwner table added
        public DbSet<AdoptionRequest> AdoptionRequests { get; set; }  // Add DbSet for AdoptionRequest

        // Configure relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship between Adoption and Pet
            modelBuilder.Entity<Adoption>()
                .HasOne(a => a.Pet)
                .WithMany() // Pet can have multiple adoptions (adjust if needed)
                .HasForeignKey(a => a.PetId);

            // Configure the relationship between Adoption and User
            modelBuilder.Entity<Adoption>()
                .HasOne(a => a.User)
                .WithMany() // User can adopt multiple pets (adjust if needed)
                .HasForeignKey(a => a.UserId);

            // Configure the many-to-many relationship between Pet and User through PetOwner
            modelBuilder.Entity<PetOwner>()
                .HasKey(po => new { po.PetId, po.UserId });  // Composite key for PetOwner

            modelBuilder.Entity<PetOwner>()
                .HasOne(po => po.Pet)
                .WithMany(p => p.PetOwners)  // Pet can have multiple owners
                .HasForeignKey(po => po.PetId);

            modelBuilder.Entity<PetOwner>()
                .HasOne(po => po.User)
                .WithMany(u => u.PetOwners)  // User can own multiple pets
                .HasForeignKey(po => po.UserId);

            // Additional configurations for AdoptionRequest model (if needed)
            modelBuilder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.Pet)
                .WithMany()  // A pet can have multiple requests
                .HasForeignKey(ar => ar.PetId);

            modelBuilder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.User)
                .WithMany()  // A user can request adoption of multiple pets
                .HasForeignKey(ar => ar.UserId);
            
            // One AdoptionRequest has one User (FK: UserId)
            modelBuilder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.User)
                .WithMany(u => u.AdoptionRequests)
                .HasForeignKey(ar => ar.UserId);

            // One AdoptionRequest has one Pet (FK: PetId)
            modelBuilder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.Pet)
                .WithMany(p => p.AdoptionRequests)
                .HasForeignKey(ar => ar.PetId);

            // Ensure table naming convention is explicit (optional)
            modelBuilder.Entity<PetOwner>().ToTable("PetOwners");
            modelBuilder.Entity<Adoption>().ToTable("Adoptions");
            modelBuilder.Entity<AdoptionRequest>().ToTable("AdoptionRequests");
        }
    }
}
