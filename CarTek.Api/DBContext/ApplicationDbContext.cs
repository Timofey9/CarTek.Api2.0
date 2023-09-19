using CarTek.Api.Model;
using CarTek.Api.Model.Orders;
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

        public DbSet<Order> Orders { get; set; } 
        public DbSet<DriverTask> DriverTasks { get; set; }
        public DbSet<DriverTaskNote> DriverTaskNotes { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }
        public DbSet<TN> TNs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>(entity =>
            {
                entity.ToTable("cars");

            });

            modelBuilder.Entity<Questionary>(entity =>
            {
                entity.ToTable("questionaries");

                entity.HasOne(e => e.Car)
                .WithMany(car => car.Questionaries);
            });

            modelBuilder.Entity<Driver>(entity =>
            {
                entity.ToTable("drivers");

                entity.HasOne(e => e.Car)
                .WithMany(car => car.Drivers);
            });
        }
    }
}
