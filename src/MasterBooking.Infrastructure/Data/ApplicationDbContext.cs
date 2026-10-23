using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;

namespace MasterBooking.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SiteSettings> SiteSettings { get; set; }
        public DbSet<GalleryImage> GalleryImages { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<WorkingHours> WorkingHours { get; set; }
        public DbSet<DayOff> DayOffs { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SiteSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Theme).HasMaxLength(50);
                entity.Property(e => e.PrimaryColor).HasMaxLength(20);
                entity.Property(e => e.CoverImage).HasMaxLength(200);
                entity.Property(e => e.Logo).HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).HasMaxLength(50);
                entity.Property(e => e.SocialLinks).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.TimeZoneId).HasDefaultValue("Asia/Vladivostok");
            });

            modelBuilder.Entity<GalleryImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImagePath).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Order).HasDefaultValue(0);
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Client)
                    .WithMany()
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Service)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.FinalPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(20).HasDefaultValue("Cash");
                entity.Property(e => e.Notes).HasMaxLength(500);
            });

            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<WorkingHours>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DayOfWeek).IsRequired();
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.EndTime).IsRequired();
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClientName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Text).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            });

            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Comment).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.HasOne(e => e.Service)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<DayOff>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Date).IsRequired();
            });
        }
    }
}
