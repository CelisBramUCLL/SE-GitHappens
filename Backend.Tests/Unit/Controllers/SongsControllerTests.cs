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
            var songs = new List<SongDTO>
            {
                new() { Id = 1, Title = "Track1", Artist = "A" },
                new() { Id = 2, Title = "Track2", Artist = "B" }
            };
            
            _serviceMock
                .Setup(s => s.GetAllAsync(null, 1, 20))
                .ReturnsAsync((songs, songs.Count, 1));

            var result = await _controller.GetAll(null, 1, 20) as OkObjectResult;

            result.Should().NotBeNull();
            var value = result!.Value!;
            value.GetType().GetProperty("totalCount")!.GetValue(value).Should().Be(2);
            value.GetType().GetProperty("totalPages")!.GetValue(value).Should().Be(1);
        }

        [Fact]
        public async Task GetById_ShouldReturnSong_WhenExists()
        {
            var song = new SongDTO { Id = 7, Title = "Hello" };
            _serviceMock.Setup(s => s.GetByIdAsync(7)).ReturnsAsync(song);

            var result = await _controller.GetById(7);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenMissing()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(9)).ReturnsAsync((SongDTO)null!);

            var result = await _controller.GetById(9);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Search_ShouldReturnOk_WithResults()
        {
            var list = new List<SongDTO> { new() { Id = 1, Title = "Hit" } };
            _serviceMock.Setup(s => s.SearchAsync("pop", 5)).ReturnsAsync(list);

            var result = await _controller.Search("pop", 5) as OkObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be(list);
        }

        [Fact]
        public async Task Search_ShouldReturnBadRequest_WhenQueryEmpty()
        {
            var result = await _controller.Search("", 5);

            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult)!.Value.Should().Be("Search query cannot be empty");
        }
    }
}