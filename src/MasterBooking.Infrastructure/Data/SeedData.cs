using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterBooking.Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            if (await context.SiteSettings.AnyAsync())
            {
                return;
            }

            var adminEmail = "admin@demo.com";
            var adminPassword = "Password123!";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, adminPassword);
            }

            var services = new List<Service>
            {
                new Service { Name = "Стрижка", Price = 1000m, IsActive = true },
                new Service { Name = "Окрашивание", Price = 3000m, IsActive = true },
                new Service { Name = "Укладка", Price = 1500m, IsActive = true }
            };
            context.Services.AddRange(services);

            var clients = new List<Client>
            {
                new Client { Name = "Алексей Петров", PhoneNumber = "+79991234567" },
                new Client { Name = "Мария Сидорова", PhoneNumber = "+79997654321" }
            };
            context.Clients.AddRange(clients);

            var workingHours = new List<WorkingHours>
            {
                new WorkingHours { DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(18, 0, 0) },
                new WorkingHours { DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(18, 0, 0) },
                new WorkingHours { DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(18, 0, 0) },
                new WorkingHours { DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(18, 0, 0) },
                new WorkingHours { DayOfWeek = DayOfWeek.Friday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(18, 0, 0) }
            };
            context.WorkingHours.AddRange(workingHours);

            var settings = new SiteSettings
            {
                DisplayName = "Иван Иванов",
                UserId = adminUser.Id,
                Theme = "Light",
                PrimaryColor = "#5B2F91",
                AboutMe = "Привет! Я профессиональный парикмахер с 10-летним стажем.",
                PhoneNumber = "+79990000000",
                SocialLinks = "https://instagram.com/ivanov",
                ShowCalendar = true,
                TimeZoneId = "Russian Standard Time"
            };
            context.SiteSettings.Add(settings);

            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    ClientId = clients[0].Id,
                    ServiceId = services[0].Id,
                    StartDateTime = DateTime.SpecifyKind(DateTime.Today.AddHours(10), DateTimeKind.Utc),
                    EndDateTime = DateTime.SpecifyKind(DateTime.Today.AddHours(11), DateTimeKind.Utc),
                    Status = "Completed",
                    FinalPrice = 1000m
                },
                new Appointment
                {
                    ClientId = clients[1].Id,
                    ServiceId = services[1].Id,
                    StartDateTime = DateTime.SpecifyKind(DateTime.Today.AddHours(14), DateTimeKind.Utc),
                    EndDateTime = DateTime.SpecifyKind(DateTime.Today.AddHours(15), DateTimeKind.Utc),
                    Status = "Completed",
                    FinalPrice = 3000m
                }
            };
            context.Appointments.AddRange(appointments);

            await context.SaveChangesAsync();
        }
    }
}
