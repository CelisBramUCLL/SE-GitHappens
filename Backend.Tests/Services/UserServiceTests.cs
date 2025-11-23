using Xunit;
using Moq;
using Dotnet_test.Domain;
using Dotnet_test.DTOs.User;
using Dotnet_test.Exceptions;
using Dotnet_test.Interfaces;
using Dotnet_test.Services;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _repoMock = new Mock<IUserRepository>();
            _service = new UserService(_repoMock.Object);
        }
        [Fact]
        public void Constructor_ShouldInitialize_WithRepository()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();

            // Act
            var service = new UserService(mockRepo.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnUsers()
        {
            // Arrange
            var users = new List<User> { new() { Id = 1, Email = "a@test.com" } };
            _repoMock.Setup(r => r.GetAll()).ReturnsAsync(users);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(users);
            _repoMock.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser()
        {
            // Arrange
            var user = new User { Id = 42, Email = "a@test.com" };
            _repoMock.Setup(r => r.GetById(42)).ReturnsAsync(user);

            // Act
            var result = await _service.GetByIdAsync(42);

            // Assert
            result.Should().Be(user);
            _repoMock.Verify(r => r.GetById(42), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnTrue_WhenRepositoryReturnsTrue()
        {
            _repoMock.Setup(r => r.Delete(10)).ReturnsAsync(true);

            var result = await _service.DeleteUserAsync(10);

            result.Should().BeTrue();
            _repoMock.Verify(r => r.Delete(10), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnResponse_WhenRepositoryReturnsLoginResponse()
        {
            var dto = new LoginDTO("user@test.com", "pass");
            var response = new LoginResponseDTO(1, "username", "jwt");
            _repoMock.Setup(r => r.Login(dto)).ReturnsAsync(response);

            var result = await _service.LoginAsync(dto);

            result.Should().BeEquivalentTo(response);
            _repoMock.Verify(r => r.Login(dto), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCallRepositoryCreate_And_ReturnCreatedUser()
        {
            // Arrange
            var dto = new CreateUserDTO
            {
                FirstName = "F",
                LastName = "L",
                Email = "test@test.com",
                Password = "secret",
                Username = "Tester",
                Role = "User"
            };
            var createdUser = new User { Id = 1, Email = dto.Email };
            _repoMock.Setup(r => r.Create(It.IsAny<User>())).ReturnsAsync(createdUser);

            // Act
            var result = await _service.CreateUserAsync(dto);

            // Assert
            result.Should().Be(createdUser);
            _repoMock.Verify(r => r.Create(It.Is<User>(u =>   
                u.Email == dto.Email &&
                u.Username == dto.Username &&
                u.FirstName == dto.FirstName &&
                u.LastName == dto.LastName
            )), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowEmailAlreadyExists_WhenRepoThrowsDuplicateEmail()
        {
            var dto = new CreateUserDTO
            {
                Email = "exists@test.com",
                Password = "123",
                Username = "x",
                FirstName = "y",
                LastName = "z",
                Role = "User"
            };

            _repoMock
                .Setup(r => r.Create(It.IsAny<User>()))
                .ThrowsAsync(new InvalidOperationException("email address already exists"));

            var act = async () => await _service.CreateUserAsync(dto);

            await act.Should().ThrowAsync<EmailAlreadyExistsException>()
                .WithMessage($"*{dto.Email}*");
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowGenericException_WhenUnknownErrorOccurs()
        {
            var dto = new CreateUserDTO
            {
                Email = "test@test.com",
                Password = "123",
                Username = "x",
                Role = "User",
                FirstName = "A",
                LastName = "B"
            };

            _repoMock
                .Setup(r => r.Create(It.IsAny<User>()))
                .ThrowsAsync(new Exception("DB failure"));

            var act = async () => await _service.CreateUserAsync(dto);

            var ex = await act.Should().ThrowAsync<Exception>();
            ex.Which.Message.Should().Be("There was an error creating the user");
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnUpdatedUser()
        {
            var dto = new UpdateUserDTO
            {
                Email = "new@test.com",
                Password = "newpass",
                Role = "User",
                Username = "newUser",
                FirstName = "John",
                LastName = "Doe"
            };
            var updatedUser = new User { Id = 2, Email = "new@test.com" };
            _repoMock.Setup(r => r.Update(It.IsAny<User>(), dto)).ReturnsAsync(updatedUser);

            var result = await _service.UpdateUserAsync(2, dto);

            result.Should().Be(updatedUser);
            _repoMock.Verify(r => r.Update(It.Is<User>(u =>
                u.Id == 2 &&
                u.Email == dto.Email &&
                u.Username == dto.Username &&
                u.Role == Role.User
            ), dto), Times.Once);
        }
    }
}