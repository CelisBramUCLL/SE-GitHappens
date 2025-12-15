using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Dotnet_test.Controllers;
using Dotnet_test.Interfaces;
using Dotnet_test.DTOs.User;
using Dotnet_test.Domain;
using Dotnet_test.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _serviceMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _serviceMock = new Mock<IUserService>();
            _controller = new UsersController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithUsers()
        {
            var users = new List<User> { new() { Id = 1, Email = "a@test.com" } };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(users);

            var result = await _controller.GetAll() as OkObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().BeEquivalentTo(users);
        }

        [Fact]
        public async Task GetUser_ShouldReturnOk_WhenUserFound()
        {
            var user = new User { Id = 5, Email = "abc@test.com" };
            _serviceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(user);

            var result = await _controller.GetUser(5);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUser_ShouldReturnNotFound_WhenNull()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(10)).ReturnsAsync((User)null!);

            var result = await _controller.GetUser(10);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateUser_ShouldReturnCreated_WhenValid()
        {
            var dto = new CreateUserDTO
            {
                Email = "z@test.com",
                Password = "p",
                Username = "tester",
                FirstName = "A",
                LastName = "B",
                Role = "User"
            };
            var created = new User { Id = 3, Email = dto.Email };
            _serviceMock.Setup(s => s.CreateUserAsync(dto)).ReturnsAsync(created);

            var result = await _controller.CreateUser(dto) as CreatedAtActionResult;

            result.Should().NotBeNull();
            result!.ActionName.Should().Be(nameof(_controller.GetUser));
            result.Value.Should().Be(created);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_ForEmailExists()
        {
            var dto = new CreateUserDTO { Email = "a@test.com" };
            _serviceMock.Setup(s => s.CreateUserAsync(dto))
                .ThrowsAsync(new EmailAlreadyExistsException("a@test.com"));

            var result = await _controller.CreateUser(dto) as BadRequestObjectResult;

            result.Should().NotBeNull();
            var msgProp = result!.Value!.GetType().GetProperty("message")!;
            msgProp.GetValue(result.Value)!.ToString().Should().Contain("a@test.com");
        }

        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_ForGeneralException()
        {
            var dto = new CreateUserDTO { Email = "b@test.com" };
            _serviceMock.Setup(s => s.CreateUserAsync(dto)).ThrowsAsync(new Exception("fail"));

            var result = await _controller.CreateUser(dto) as BadRequestObjectResult;

            result.Should().NotBeNull();
            var msg = result!.Value!.GetType().GetProperty("message")!.GetValue(result.Value) as string;
            msg.Should().Contain("fail");
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenUpdated()
        {
            var dto = new UpdateUserDTO { Username = "new" };
            var user = new User { Id = 1, Username = "new" };
            _serviceMock.Setup(s => s.UpdateUserAsync(1, dto)).ReturnsAsync(user);

            var result = await _controller.UpdateUser(1, dto) as OkObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be(user);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnNotFound_WhenNull()
        {
            var dto = new UpdateUserDTO { Username = "x" };
            _serviceMock.Setup(s => s.UpdateUserAsync(9, dto)).ReturnsAsync((User)null!);

            var result = await _controller.UpdateUser(9, dto) as NotFoundObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be("User not found");
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnOk_WhenDeleted()
        {
            _serviceMock.Setup(s => s.DeleteUserAsync(5)).ReturnsAsync(true);

            var result = await _controller.DeleteUser(5) as OkObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be("User with id 5 deleted");
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNotFound_WhenMissing()
        {
            _serviceMock.Setup(s => s.DeleteUserAsync(7)).ReturnsAsync(false);

            var result = await _controller.DeleteUser(7) as NotFoundObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be("User with id 7 not found");
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenValid()
        {
            var dto = new LoginDTO("x@test.com", "pass");
            var resp = new LoginResponseDTO(1, "user", "jwt");
            _serviceMock.Setup(s => s.LoginAsync(dto)).ReturnsAsync(resp);
            var action = await _controller.Login(dto);
            var result = action.Result as OkObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be(resp);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenInvalid()
        {
            var dto = new LoginDTO("bad@test.com", "wrong");
            _serviceMock.Setup(s => s.LoginAsync(dto)).ReturnsAsync((LoginResponseDTO)null!);

            var action = await _controller.Login(dto);
            var result = action.Result as UnauthorizedObjectResult;

            result.Should().NotBeNull();
            var val = result!.Value!.GetType().GetProperty("message")!.GetValue(result.Value) as string;
            val.Should().Contain("Invalid email or password");
        }
    }
}