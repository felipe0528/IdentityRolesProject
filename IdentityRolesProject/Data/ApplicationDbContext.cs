using IdentityRolesProject.Models.Localization;
using IdentityRolesProject.Models.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityRolesProject.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<DocType> DocType { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            //MANY TO MANY EXAMPLE
            //builder.Entity<ChallengesAdmins>()
            //    .HasKey(t => new { t.ChallengeId, t.UserId });
            //builder.Entity<ChallengesAdmins>()
            //    .HasOne(pt => pt.Challenge)
            //    .WithMany(p => p.ChallengeAdmins)
            //    .HasForeignKey(pt => pt.ChallengeId);
            //builder.Entity<ChallengesAdmins>()
            //    .HasOne(pt => pt.User)
            //    .WithMany(t => t.ChallengesAsAdmin)
            //    .HasForeignKey(pt => pt.UserId);
        }
    }
}
