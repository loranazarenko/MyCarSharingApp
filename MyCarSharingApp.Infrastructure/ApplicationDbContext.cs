using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyCarSharingApp.Domain.Entities;

namespace MyCarSharingApp.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // enum <-> string for Car.Type
            var carTypeConverter = new EnumToStringConverter<CarType>();

            modelBuilder.Entity<Car>()
                .Property(c => c.Type)
                .HasConversion(carTypeConverter)
                .HasMaxLength(50); 
        }
    }
}
