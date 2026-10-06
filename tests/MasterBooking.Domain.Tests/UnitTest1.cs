using MasterBooking.Domain.Entities;
using Xunit;

namespace MasterBooking.Domain.Tests
{
    public class MasterTests
    {
        [Fact]
        public void Master_CanBeCreated()
        {
            var master = new Master
            {
                Id = 1,
                Slug = "test-master",
                DisplayName = "Test Master",
                UserId = "user-id"
            };

            Assert.Equal(1, master.Id);
            Assert.Equal("test-master", master.Slug);
            Assert.Equal("Test Master", master.DisplayName);
            Assert.Equal("user-id", master.UserId);
        }
    }
}