using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;

namespace MasterBooking.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        private readonly IMasterProvider _masterProvider;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMasterProvider masterProvider)
            : base(options)
        {
            _masterProvider = masterProvider;
        }

        public DbSet<Master> Masters { get; set; }
        public DbSet<SiteSettings> SiteSettings { get; set; }
        public DbSet<GalleryImage> GalleryImages { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<WorkingHours> WorkingHours { get; set; }
        public DbSet<DayOff> DayOffs { get; set; }
        public int? CurrentMasterId => _masterProvider.GetMasterId();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Master>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UserId).IsRequired();
            });

            modelBuilder.Entity<SiteSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Master)
                    .WithMany()
                    .HasForeignKey(e => e.MasterId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.Theme).HasMaxLength(50);
                entity.Property(e => e.PrimaryColor).HasMaxLength(20);
                entity.Property(e => e.CoverImage).HasMaxLength(200);
                entity.Property(e => e.Logo).HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).HasMaxLength(50);
                entity.Property(e => e.SocialLinks).HasMaxLength(200);

                entity.HasQueryFilter(s => CurrentMasterId == null || s.MasterId == CurrentMasterId);
            });

            modelBuilder.Entity<GalleryImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Master)
                    .WithMany()
                    .HasForeignKey(e => e.MasterId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.ImagePath).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Order).HasDefaultValue(0);

                entity.HasQueryFilter(g => CurrentMasterId == null || g.MasterId == CurrentMasterId);
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Master)
                    .WithMany()
                    .HasForeignKey(e => e.MasterId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");

                entity.HasQueryFilter(s => CurrentMasterId == null || s.MasterId == CurrentMasterId);
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Master)
                    .WithMany()
                    .HasForeignKey(e => e.MasterId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(50);

                entity.HasQueryFilter(c => CurrentMasterId == null || c.MasterId == CurrentMasterId);
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Master)
                    .WithMany()
                    .HasForeignKey(e => e.MasterId)
                    .OnDelete(DeleteBehavior.Cascade);
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

                entity.HasQueryFilter(a => CurrentMasterId == null || a.MasterId == CurrentMasterId);
            });

            modelBuilder.Entity<WorkingHours>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Master)
                    .WithMany()
                    .HasForeignKey(e => e.MasterId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.DayOfWeek).IsRequired();
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.EndTime).IsRequired();

                entity.HasQueryFilter(w => CurrentMasterId == null || w.MasterId == CurrentMasterId);
            });

            modelBuilder.Entity<DayOff>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Master)
                    .WithMany()
                    .HasForeignKey(e => e.MasterId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.Date).IsRequired();

                entity.HasQueryFilter(d => CurrentMasterId == null || d.MasterId == CurrentMasterId);
            });
        }
    }
}