using CarTek.Api.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarTek.Api.DBContext
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }   
        
        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Trailer> Trailers { get; set; }
        public DbSet<Questionary> Questionaries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>(entity =>
            {
                entity.ToTable("cars");

                entity.HasOne(e => e.Trailer)
                .WithOne(e => e.Car)
                .HasForeignKey<Trailer>(trailer => trailer.CarId);

                entity.HasOne(e => e.Driver)
                .WithOne(e => e.Car)
                .HasForeignKey<Driver>(driver => driver.CarId);
            });

            modelBuilder.Entity<Questionary>(entity =>
            {
                entity.ToTable("questionaries");

                entity.HasOne(e => e.Car)
                .WithMany(questions => questions.Questionaries);
            });
        }
    }
}
