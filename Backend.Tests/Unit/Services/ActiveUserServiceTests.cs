using Dotnet_test.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Unit.Services
{
    public class ActiveUserServiceTests
    {
        private readonly Mock<ILogger<ActiveUserService>> _loggerMock;
        private readonly ActiveUserService _service;

        public ActiveUserServiceTests()
        {
            _loggerMock = new Mock<ILogger<ActiveUserService>>();
            _service = new ActiveUserService(_loggerMock.Object);
        }

        [Fact]
        public void AddUser_ShouldLogWarning_WhenInvalidParams()
        {
            _service.AddUser(0, null!);
            _service.AddUser(-1, " ");
            _loggerMock.Verify(
                log => log.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void AddUser_ShouldAddNewUser()
        {
            _service.AddUser(1, "A123");

            Assert.True(_service.IsUserActive(1));
            Assert.Equal("A123", _service.GetConnectionId(1));
            Assert.Equal(1, _service.GetActiveUserCount());
        }

        [Fact]
        public void AddUser_ShouldReplaceExistingConnection()
        {
            _service.AddUser(2, "A");
            _service.AddUser(2, "B"); // reconnect same user

            Assert.Equal("B", _service.GetConnectionId(2));
            Assert.Equal(1, _service.GetActiveUserCount());
        }

        [Fact]
        public void RemoveUserByConnection_ShouldReturnFalse_ForInvalidInput()
        {
            var result = _service.RemoveUserByConnection(" ");
            Assert.False(result);
        }

        [Fact]
        public void RemoveUserByConnection_ShouldRemoveExistingUser()
        {
            _service.AddUser(3, "Conn3");
            var success = _service.RemoveUserByConnection("Conn3");

            Assert.True(success);
            Assert.False(_service.IsUserActive(3));
            Assert.Equal(0, _service.GetActiveUserCount());
        }

        [Fact]
        public void RemoveUserByConnection_ShouldReturnFalse_WhenConnectionNotFound()
        {
            var result = _service.RemoveUserByConnection("DoesNotExist");
            Assert.False(result);
        }

        [Fact]
        public void RemoveUserById_ShouldReturnFalse_ForInvalidUserId()
        {
            var result = _service.RemoveUserById(0);
            Assert.False(result);
        }

        [Fact]
        public void RemoveUserById_ShouldRemoveAndReturnTrue_WhenUserExists()
        {
            _service.AddUser(5, "C5");
            var result = _service.RemoveUserById(5);

            Assert.True(result);
            Assert.False(_service.IsUserActive(5));
        }

        [Fact]
        public void GetActiveUserIds_ShouldReturnAllUserIds()
        {
            _service.AddUser(10, "Conn10");
            _service.AddUser(11, "Conn11");

            var users = _service.GetActiveUserIds().ToList();

            Assert.Contains(10, users);
            Assert.Contains(11, users);
        }

        [Fact]
        public void GetConnectionId_ShouldReturnConnection_WhenExists()
        {
            _service.AddUser(20, "Conn20");

            var result = _service.GetConnectionId(20);

            Assert.Equal("Conn20", result);
        }

        [Fact]
        public void GetConnectionId_ShouldReturnNull_WhenNotFound()
        {
            var result = _service.GetConnectionId(999);
            Assert.Null(result);
        }
    }
}