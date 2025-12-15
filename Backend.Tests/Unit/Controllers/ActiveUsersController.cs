using Dotnet_test.Controllers;
using Dotnet_test.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests.Unit.Controllers
{
    public class ActiveUsersControllerTests
    {
        private readonly Mock<IActiveUserService> _serviceMock;
        private readonly ActiveUsersController _controller;

        public ActiveUsersControllerTests()
        {
            _serviceMock = new Mock<IActiveUserService>();
            _controller = new ActiveUsersController(_serviceMock.Object);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ActiveUsersController(null!));
        }

        [Fact]
        public void GetActiveUserCount_ShouldReturnOk_WithCountValue()
        {
            _serviceMock.Setup(s => s.GetActiveUserCount()).Returns(7);

            var result = _controller.GetActiveUserCount() as OkObjectResult;

            Assert.NotNull(result);
            var obj = result!.Value!;
            var prop = obj.GetType().GetProperty("activeUserCount");
            Assert.NotNull(prop);

            var count = prop!.GetValue(obj);
            Assert.Equal(7, count);
        }

        [Fact]
        public void GetActiveUsers_ShouldReturnOk_WithIdsAndCount()
        {
            var ids = new[] { 1, 2, 3 };
            _serviceMock.Setup(s => s.GetActiveUserIds()).Returns(ids);

            var result = _controller.GetActiveUsers();

            var ok = Assert.IsType<OkObjectResult>(result);
            var body = ok.Value!;
            var propCount = body.GetType().GetProperty("count")!.GetValue(body);
            Assert.Equal(3, propCount);
        }

        [Fact]
        public void CheckUserStatus_ShouldReturnOk_WithExpectedFlag()
        {
            _serviceMock.Setup(s => s.IsUserActive(99)).Returns(true);

            var result = _controller.CheckUserStatus(99);

            var ok = Assert.IsType<OkObjectResult>(result);
            var body = ok.Value!;
            var isActiveProp = body.GetType().GetProperty("isActive")!.GetValue(body);
            Assert.True((bool)isActiveProp!);
        }
    }
}