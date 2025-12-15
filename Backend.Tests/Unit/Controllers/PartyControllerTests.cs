using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Dotnet_test.Controllers;
using Dotnet_test.DTOs.Party;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Song;
using Dotnet_test.DTOs.Playlist;
using Dotnet_test.DTOs.User;
using Dotnet_test.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Backend.Tests.Controllers
{
    public class PartyControllerTests
    {
        private readonly Mock<IPartyService> _serviceMock;
        private readonly PartyController _controller;

        public PartyControllerTests()
        {
            _serviceMock = new Mock<IPartyService>();
            _controller = new PartyController(_serviceMock.Object);
        }

        private void SetUserId(int id)
        {
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()) })
            );
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        private void SetEmptyHttpContext()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithParties()
        {
            var expected = new List<PartyDTO> { new() { Id = 1, Name = "P" } };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(expected);

            var result = await _controller.GetAll() as OkObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenFound()
        {
            var party = new PartyDTO { Id = 1, Name = "Found" };
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(party);

            var result = await _controller.GetById(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenNull()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((PartyDTO)null!);

            var result = await _controller.GetById(1);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateParty_ShouldReturnUnauthorized_WhenNoUserId()
        {
            SetEmptyHttpContext();

            var result = await _controller.CreateParty(new CreatePartyDTO("Party"));

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task CreateParty_ShouldReturnCreated_WhenValid()
        {
            SetUserId(5);
            var dto = new CreatePartyDTO("Party");
            var created = new PartyDTO { Id = 42, Name = dto.Name };
            _serviceMock.Setup(s => s.CreatePartyAsync(dto, 5)).ReturnsAsync(created);

            var result = await _controller.CreateParty(dto) as CreatedAtActionResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be(created);
            result.ActionName.Should().Be(nameof(_controller.GetById));
        }

        [Fact]
        public async Task CreateParty_ShouldReturnBadRequest_WhenExceptionThrown()
        {
            SetUserId(2);
            var dto = new CreatePartyDTO("Bad");
            _serviceMock.Setup(s => s.CreatePartyAsync(dto, 2))
                .ThrowsAsync(new Exception("fail"));

            var result = await _controller.CreateParty(dto) as BadRequestObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().BeOfType<string>().Which.Should().Contain("fail");
        }

        [Fact]
        public async Task Update_ShouldReturnOk_WhenPartyUpdated()
        {
            var dto = new UpdatePartyDTO { Name = "New" };
            _serviceMock.Setup(s => s.UpdatePartyAsync(1, dto))
                .ReturnsAsync(new PartyDTO { Id = 1, Name = "New" });

            var result = await _controller.Update(1, dto);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenNull()
        {
            var dto = new UpdatePartyDTO { Name = "X" };
            _serviceMock.Setup(s => s.UpdatePartyAsync(1, dto))
                .ReturnsAsync((PartyDTO)null!);

            var result = await _controller.Update(1, dto);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnUnauthorized_WhenNoUserId()
        {
            SetEmptyHttpContext();

            var result = await _controller.Delete(1);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnOk_WhenDeleted()
        {
            SetUserId(9);
            _serviceMock.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(new PartyDTO { Id = 1, HostUser = new HostUserDTO(9, "U") });
            _serviceMock.Setup(s => s.DeletePartyAsync(1, 9)).ReturnsAsync(true);

            var result = await _controller.Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenMissing()
        {
            SetUserId(7);
            _serviceMock.Setup(s => s.DeletePartyAsync(1, 7)).ReturnsAsync(false);
            _serviceMock.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync((PartyDTO)null!);

            var result = await _controller.Delete(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task JoinParty_ShouldReturnUnauthorized_WhenNoUserId()
        {
            SetEmptyHttpContext();

            var result = await _controller.JoinParty(new JoinPartyDTO(1));

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task JoinParty_ShouldReturnOk_WhenValid()
        {
            SetUserId(10);
            var dto = new JoinPartyDTO(1);
            var participant = new ParticipantDTO { Id = 1, PartyId = 1, UserId = 10 };
            _serviceMock.Setup(s => s.JoinPartyAsync(dto, 10)).ReturnsAsync(participant);

            var result = await _controller.JoinParty(dto) as OkObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be(participant);
        }

        [Fact]
        public async Task JoinParty_ShouldReturnBadRequest_WhenException()
        {
            SetUserId(5);
            var dto = new JoinPartyDTO(1);
            _serviceMock.Setup(s => s.JoinPartyAsync(dto, 5))
                .ThrowsAsync(new Exception("boom"));

            var result = await _controller.JoinParty(dto) as BadRequestObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be("boom");
        }

        [Fact]
        public async Task LeaveParty_ShouldReturnNotFound_WhenNullParticipant()
        {
            SetUserId(2);
            _serviceMock.Setup(s => s.LeavePartyAsync(1, 2))
                .ReturnsAsync((ParticipantInPartyDTO)null!);

            var result = await _controller.LeaveParty(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task LeaveParty_ShouldReturnOk_WhenParticipantFound()
        {
            SetUserId(2);
            var dto = new ParticipantInPartyDTO { Id = 1, UserId = 2, UserName = "U" };
            _serviceMock.Setup(s => s.LeavePartyAsync(1, 2)).ReturnsAsync(dto);

            var result = await _controller.LeaveParty(1) as OkObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be(dto);
        }

        [Fact]
        public async Task AddSong_ShouldReturnUnauthorized_WhenNoUserId()
        {
            SetEmptyHttpContext();

            var result = await _controller.AddSong(new AddSongDTO(1));

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task AddSong_ShouldReturnOk_WhenValid()
        {
            SetUserId(3);
            var dto = new AddSongDTO(1);
            var song = new PlaylistSongDTO { Id = 1, SongId = 1 };
            _serviceMock.Setup(s => s.AddSongAsync(dto, 3)).ReturnsAsync(song);

            var result = await _controller.AddSong(dto) as OkObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be(song);
        }

        [Fact]
        public async Task AddSong_ShouldReturnBadRequest_WhenFails()
        {
            SetUserId(4);
            var dto = new AddSongDTO(1);
            _serviceMock.Setup(s => s.AddSongAsync(dto, 4))
                .ThrowsAsync(new Exception("fail"));

            var result = await _controller.AddSong(dto) as BadRequestObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be("fail");
        }

        [Fact]
        public async Task RemoveSong_ShouldReturnUnauthorized_WhenNoUserId()
        {
            SetEmptyHttpContext();

            var result = await _controller.RemoveSong(new RemoveSongDTO(1));

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task RemoveSong_ShouldReturnOk_WhenValid()
        {
            SetUserId(1);
            var dto = new RemoveSongDTO(2);
            var song = new PlaylistSongDTO { Id = 99 };
            _serviceMock.Setup(s => s.RemoveSongAsync(dto, 1)).ReturnsAsync(song);

            var result = await _controller.RemoveSong(dto) as OkObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be(song);
        }

        [Fact]
        public async Task RemoveSong_ShouldReturnBadRequest_WhenException()
        {
            SetUserId(1);
            var dto = new RemoveSongDTO(5);
            _serviceMock.Setup(s => s.RemoveSongAsync(dto, 1))
                .ThrowsAsync(new Exception("oops"));

            var result = await _controller.RemoveSong(dto) as BadRequestObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be("oops");
        }

        [Fact]
        public async Task GetMyActiveParty_ShouldReturnUnauthorized_WhenNoUserId()
        {
            SetEmptyHttpContext();

            var result = await _controller.GetMyActiveParty();

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetMyActiveParty_ShouldReturnOk_WhenFound()
        {
            SetUserId(7);
            var party = new PartyDTO { Id = 11, Name = "Active" };
            _serviceMock.Setup(s => s.GetMyActivePartyAsync(7)).ReturnsAsync(party);

            var response = await _controller.GetMyActiveParty() as OkObjectResult;

            response.Should().NotBeNull();
            var body = response!.Value!;
            var prop = body.GetType().GetProperty("hasActiveParty")!;
            ((bool)prop.GetValue(body)!).Should().BeTrue();
        }

        [Fact]
        public async Task GetMyActiveParty_ShouldReturnOk_WhenNone()
        {
            SetUserId(8);
            _serviceMock.Setup(s => s.GetMyActivePartyAsync(8))
                        .ReturnsAsync((PartyDTO)null!);

            var response = await _controller.GetMyActiveParty() as OkObjectResult;

            response.Should().NotBeNull();
            var body = response!.Value!;
            var prop = body.GetType().GetProperty("hasActiveParty")!;
            ((bool)prop.GetValue(body)!).Should().BeFalse();
        }
    }
}