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
        private const string TimeZoneId = "Asia/Vladivostok";

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

            var zone = ResolveZone();
            var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone).Date;
            var monday = todayLocal.AddDays(-(((int)todayLocal.DayOfWeek + 6) % 7));

            DateTime At(DateTime day, int hour) =>
                TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(day.AddHours(hour), DateTimeKind.Unspecified), zone);


            var haircut = new Service { Name = "Стрижка", Price = 1500m, Duration = 60, IsActive = true, IsVisible = true };
            var coloring = new Service { Name = "Окрашивание", Price = 4500m, Duration = 120, IsActive = true, IsVisible = true };
            var styling = new Service { Name = "Укладка", Price = 1200m, Duration = 60, IsActive = true, IsVisible = true };
            context.Services.AddRange(haircut, coloring, styling);


            var anna = new Client { Name = "Алина Морозова", PhoneNumber = "+7 900 111-22-33", Notes = "Окрашивание: оттенок 8.1, пропорция 1:1.5. Без аммиака.", MasterId = adminUser.Id };
            var maria = new Client { Name = "Мария Белова", PhoneNumber = "+7 900 444-55-66", Notes = "Предпочитает утренние записи.", MasterId = adminUser.Id };
            context.Clients.AddRange(anna, maria);


            foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday })
            {
                context.WorkingHours.Add(new WorkingHours { DayOfWeek = day, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(19, 0, 0) });
            }


            context.SiteSettings.Add(new SiteSettings
            {
                DisplayName = "Анна Соколова",
                UserId = adminUser.Id,
                Theme = "Light",
                PrimaryColor = "#5B2F91",
                CoverImage = "/demo/cover.svg",
                AboutMe = "Мастер с десятилетним опытом. Подбираю стрижку и цвет под вас, без лишних процедур.",
                City = "Владивосток",
                Address = "ул. Светланская, 10, вход со двора",
                PhoneNumber = "+7 900 000-00-00",
                SocialLinks = "@anna_sokolova_hair",
                ShowCalendar = true,
                IsPublished = true,
                TimeZoneId = TimeZoneId
            });


            Appointment Appt(Client client, Service service, DateTime startUtc, string status, decimal price, string payment, string notes = "") => new()
            {
                Client = client,
                Service = service,
                StartDateTime = startUtc,
                EndDateTime = startUtc.AddMinutes(service.Duration),
                Status = status,
                FinalPrice = price,
                PaymentMethod = payment,
                Notes = notes
            };

            context.Appointments.AddRange(

                Appt(anna, haircut, At(monday.AddDays(-14), 11), "Done", 1500m, "Cash"),
                Appt(maria, coloring, At(monday.AddDays(-12), 14), "Done", 4500m, "Card"),
                Appt(anna, styling, At(monday.AddDays(-8), 12), "Done", 1200m, "Transfer"),
                Appt(maria, haircut, At(monday.AddDays(-5), 15), "Done", 1500m, "Cash"),

                Appt(anna, haircut, At(monday, 10), "Done", 1500m, "Card"),
                Appt(maria, coloring, At(monday.AddDays(1), 12), "Done", 4500m, "Transfer"),
                Appt(anna, styling, At(monday.AddDays(2), 14), "Planned", 1200m, "Cash"),
                Appt(maria, haircut, At(monday.AddDays(3), 10), "Planned", 1500m, "Cash"),
                Appt(anna, coloring, At(monday.AddDays(4), 16), "Cancelled", 4500m, "Cash"));


            context.Expenses.AddRange(
                new Expense { Title = "Краска и оксиды", Amount = 3200m, Date = At(monday.AddDays(-9), 0) },
                new Expense { Title = "Аренда кресла", Amount = 5000m, Date = At(new DateTime(todayLocal.Year, todayLocal.Month, 1), 0) },
                new Expense { Title = "Расходники", Amount = 900m, Date = At(monday.AddDays(-2), 0) });


            for (var i = 1; i <= 5; i++)
            {
                context.GalleryImages.Add(new GalleryImage { ImagePath = $"/demo/work-{i}.svg", Order = i });
            }


            context.Reviews.AddRange(
                new Review { ClientName = "Алина Морозова", Text = "Попала к Анне по рекомендации и осталась. Цвет ровно такой, как хотела, и волосы в порядке.", Status = Review.Published, CreatedAt = DateTime.UtcNow.AddDays(-20) },
                new Review { ClientName = "Мария Белова", Text = "Очень аккуратная работа и приятная атмосфера. Записываюсь снова.", Status = Review.Published, CreatedAt = DateTime.UtcNow.AddDays(-9) },
                new Review { ClientName = "Екатерина", Text = "Понравилась стрижка, но пришлось немного подождать начала.", Status = Review.Pending, CreatedAt = DateTime.UtcNow.AddDays(-1) });


            context.ServiceRequests.AddRange(
                new ServiceRequest { Name = "Ольга", Phone = "+7 900 777-88-99", Comment = "Хочу записаться на окрашивание в пятницу.", Service = coloring, Status = ServiceRequest.New, CreatedAt = DateTime.UtcNow.AddHours(-5) },
                new ServiceRequest { Name = "Дарья", Phone = "+7 900 222-33-44", Comment = string.Empty, Service = haircut, Status = ServiceRequest.Contacted, CreatedAt = DateTime.UtcNow.AddDays(-2) });

            await context.SaveChangesAsync();
        }

        private static TimeZoneInfo ResolveZone()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId); }
            catch (Exception) { return TimeZoneInfo.Utc; }
        }
    }
}
