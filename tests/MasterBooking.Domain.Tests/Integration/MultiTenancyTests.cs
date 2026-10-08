using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Infrastructure.Data;
using Moq;
using Xunit;

namespace MasterBooking.Domain.Tests.Integration
{
    public class MultiTenancyTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly Mock<IMasterProvider> _masterProviderMock;

        public MultiTenancyTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _masterProviderMock = new Mock<IMasterProvider>();
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }

        private ApplicationDbContext CreateContext()
        {
            return new ApplicationDbContext(_options, _masterProviderMock.Object);
        }

        [Fact]
        public async Task Services_AreFilteredByMasterId()
        {
            var master1Id = 1;
            var master2Id = 2;
            var userId1 = "user-1";
            var userId2 = "user-2";

            using (var context = CreateContext())
            {
                context.Database.EnsureCreated();

                context.Masters.AddRange(
                    new Master { Id = master1Id, UserId = userId1, DisplayName = "Master 1", Slug = "m1" },
                    new Master { Id = master2Id, UserId = userId2, DisplayName = "Master 2", Slug = "m2" }
                );

                context.Services.AddRange(
                    new Service { Id = 1, MasterId = master1Id, Name = "Service 1", Price = 100, Duration = 30 },
                    new Service { Id = 2, MasterId = master2Id, Name = "Service 2", Price = 200, Duration = 60 }
                );

                await context.SaveChangesAsync();
            }

            _masterProviderMock.Setup(m => m.GetMasterId()).Returns(master1Id);

            using (var context = CreateContext())
            {
                var services = await context.Services.ToListAsync();
                Assert.Single(services);
                Assert.Equal(master1Id, services[0].MasterId);
                Assert.Equal("Service 1", services[0].Name);
            }
        }

        [Fact]
        public async Task Clients_AreFilteredByMasterId()
        {
            var master1Id = 1;
            var master2Id = 2;
            var userId1 = "user-1";
            var userId2 = "user-2";

            using (var context = CreateContext())
            {
                context.Database.EnsureCreated();

                context.Masters.AddRange(
                    new Master { Id = master1Id, UserId = userId1, DisplayName = "Master 1", Slug = "m1" },
                    new Master { Id = master2Id, UserId = userId2, DisplayName = "Master 2", Slug = "m2" }
                );

                context.Clients.AddRange(
                    new Client { Id = 1, MasterId = master1Id, Name = "Client 1", PhoneNumber = "111" },
                    new Client { Id = 2, MasterId = master2Id, Name = "Client 2", PhoneNumber = "222" }
                );

                await context.SaveChangesAsync();
            }

            _masterProviderMock.Setup(m => m.GetMasterId()).Returns(master1Id);

            using (var context = CreateContext())
            {
                var clients = await context.Clients.ToListAsync();
                Assert.Single(clients);
                Assert.Equal(master1Id, clients[0].MasterId);
                Assert.Equal("Client 1", clients[0].Name);
            }
        }
    }
}
