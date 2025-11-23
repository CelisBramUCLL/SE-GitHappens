using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Dotnet_test.Controllers;
using Dotnet_test.Interfaces;
using Dotnet_test.DTOs.Song;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Tests.Controllers
{
    public class SongsControllerTests
    {
        private readonly Mock<ISongService> _serviceMock;
        private readonly SongsController _controller;

        public SongsControllerTests()
        {
            _serviceMock = new Mock<ISongService>();
            _controller = new SongsController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnPagedSongs()
        {
            // Arrange
            var songs = new List<SongDTO>
            {
                new() { Id = 1, Title = "Track1", Artist = "A" },
                new() { Id = 2, Title = "Track2", Artist = "B" }
            };
            _serviceMock
                .Setup(s => s.GetAllAsync(null, 1, 20))
                .ReturnsAsync((songs, songs.Count, 1));

            // Act
            var result = await _controller.GetAll(null, 1, 20) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            var value = result!.Value!;
            value.GetType().GetProperty("totalCount")!.GetValue(value).Should().Be(2);
            value.GetType().GetProperty("totalPages")!.GetValue(value).Should().Be(1);
        }

        [Fact]
        public async Task GetById_ShouldReturnSong_WhenExists()
        {
            // Arrange
            var song = new SongDTO { Id = 7, Title = "Hello" };
            _serviceMock.Setup(s => s.GetByIdAsync(7)).ReturnsAsync(song);

            // Act
            var result = await _controller.GetById(7);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenMissing()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetByIdAsync(9)).ReturnsAsync((SongDTO)null!);

            // Act
            var result = await _controller.GetById(9);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Search_ShouldReturnOk_WithResults()
        {
            // Arrange
            var list = new List<SongDTO> { new() { Id = 1, Title = "Hit" } };
            _serviceMock.Setup(s => s.SearchAsync("pop", 5)).ReturnsAsync(list);

            // Act
            var result = await _controller.Search("pop", 5) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be(list);
        }

        [Fact]
        public async Task Search_ShouldReturnBadRequest_WhenQueryEmpty()
        {
            // Act
            var result = await _controller.Search("", 5);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult)!.Value.Should().Be("Search query cannot be empty");
        }
    }
}