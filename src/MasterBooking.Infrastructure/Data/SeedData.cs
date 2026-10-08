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

            if (await context.Masters.AnyAsync())
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

            var master = new Master
            {
                DisplayName = "Иван Иванов",
                Slug = "ivan-ivanov",
                UserId = adminUser.Id
            };
            context.Masters.Add(master);
            await context.SaveChangesAsync();

            var services = new List<Service>
            {
                new Service { MasterId = master.Id, Name = "Стрижка", Price = 1000m, IsActive = true },
                new Service { MasterId = master.Id, Name = "Окрашивание", Price = 3000m, IsActive = true },
                new Service { MasterId = master.Id, Name = "Укладка", Price = 1500m, IsActive = true }
            };
            context.Services.AddRange(services);

            var clients = new List<Client>
            {
                new Client { MasterId = master.Id, Name = "Алексей Петров", PhoneNumber = "+79991234567" },
                new Client { MasterId = master.Id, Name = "Мария Сидорова", PhoneNumber = "+79997654321" }
            };
            context.Clients.AddRange(clients);

            var workingHours = new List<WorkingHours>
            {
                new WorkingHours { MasterId = master.Id, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(18, 0, 0) },
                new WorkingHours { MasterId = master.Id, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(18, 0, 0) },
                new WorkingHours { MasterId = master.Id, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(18, 0, 0) },
                new WorkingHours { MasterId = master.Id, DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(18, 0, 0) },
                new WorkingHours { MasterId = master.Id, DayOfWeek = DayOfWeek.Friday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(18, 0, 0) }
            };
            context.WorkingHours.AddRange(workingHours);

            var settings = new SiteSettings
            {
                MasterId = master.Id,
                Theme = "Light",
                PrimaryColor = "#007bff",
                AboutMe = "Привет! Я профессиональный парикмахер с 10-летним стажем.",
                PhoneNumber = "+79990000000",
                SocialLinks = "https://instagram.com/ivanov",
                ShowCalendar = true
            };
            context.SiteSettings.Add(settings);

            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    MasterId = master.Id,
                    ClientId = clients[0].Id,
                    ServiceId = services[0].Id,
                    StartDateTime = DateTime.Today.AddHours(10),
                    EndDateTime = DateTime.Today.AddHours(11),
                    Status = "Completed",
                    FinalPrice = 1000m
                },
                new Appointment
                {
                    MasterId = master.Id,
                    ClientId = clients[1].Id,
                    ServiceId = services[1].Id,
                    StartDateTime = DateTime.Today.AddHours(14),
                    EndDateTime = DateTime.Today.AddHours(15),
                    Status = "Completed",
                    FinalPrice = 3000m
                }
            };
            context.Appointments.AddRange(appointments);

            await context.SaveChangesAsync();
        }
    }
}
