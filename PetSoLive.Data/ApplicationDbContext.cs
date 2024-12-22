using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;

namespace PetSoLive.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Adoption> Adoptions { get; set; }
        public DbSet<Assistance> Assistances { get; set; }
        public DbSet<PetOwner> PetOwners { get; set; }
        public DbSet<AdoptionRequest> AdoptionRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Adoption>()
                .HasOne(a => a.Pet)
                .WithMany()
                .HasForeignKey(a => a.PetId);

            modelBuilder.Entity<Adoption>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<PetOwner>()
                .HasKey(po => new { po.PetId, po.UserId });

            modelBuilder.Entity<PetOwner>()
                .HasOne(po => po.Pet)
                .WithMany(p => p.PetOwners)
                .HasForeignKey(po => po.PetId);

            modelBuilder.Entity<PetOwner>()
                .HasOne(po => po.User)
                .WithMany(u => u.PetOwners)
                .HasForeignKey(po => po.UserId);

            modelBuilder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.Pet)
                .WithMany()
                .HasForeignKey(ar => ar.PetId);

            modelBuilder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.User)
                .WithMany()
                .HasForeignKey(ar => ar.UserId);

            modelBuilder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.User)
                .WithMany(u => u.AdoptionRequests)
                .HasForeignKey(ar => ar.UserId);

            modelBuilder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.Pet)
                .WithMany(p => p.AdoptionRequests)
                .HasForeignKey(ar => ar.PetId);

            modelBuilder.Entity<PetOwner>().ToTable("PetOwners");
            modelBuilder.Entity<Adoption>().ToTable("Adoptions");
            modelBuilder.Entity<AdoptionRequest>().ToTable("AdoptionRequests");
        }
    }
}
