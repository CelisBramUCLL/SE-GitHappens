using Xunit;
using Moq;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dotnet_test.DTOs.Song;
using Dotnet_test.Interfaces;
using Dotnet_test.Services;
using System;

namespace Backend.Tests.Services
{
    public class SongServiceTests
    {
        private readonly Mock<ISongRepository> _repoMock;
        private readonly SongService _service;

        public SongServiceTests()
        {
            _repoMock = new Mock<ISongRepository>();
            _service = new SongService(_repoMock.Object);
        }

        // ---------- Constructor tests ----------

        [Fact]
        public void Constructor_ShouldInitialize_WithRepository()
        {
            var mockRepo = new Mock<ISongRepository>();
            var service = new SongService(mockRepo.Object);

            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenRepositoryIsNull()
        {
            Action act = () => new SongService(null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("songRepository");
        }

        // ---------- Method tests ----------

        [Fact]
        public async Task GetByIdAsync_ShouldReturnSong()
        {
            // Arrange
            var song = new SongDTO { Id = 7, Title = "Track", Artist = "Band" };
            _repoMock.Setup(r => r.GetById(7)).ReturnsAsync(song);

            // Act
            var result = await _service.GetByIdAsync(7);

            // Assert
            result.Should().Be(song);
            _repoMock.Verify(r => r.GetById(7), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_ShouldReturnListOfSongs()
        {
            // Arrange
            var songs = new List<SongDTO>
            {
                new() { Id = 1, Title = "Hello", Artist = "Artist" },
                new() { Id = 2, Title = "Goodbye", Artist = "Artist" }
            };

            _repoMock.Setup(r => r.Search("test", 5)).ReturnsAsync(songs);

            // Act
            var result = await _service.SearchAsync("test", 5);

            // Assert
            result.Should().BeEquivalentTo(songs);
            _repoMock.Verify(r => r.Search("test", 5), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnPaginatedResults()
        {
            // Arrange
            var expectedSongs = new List<SongDTO>
            {
                new() { Id = 1, Title = "Song1", Artist = "A" }
            };
            _repoMock
                .Setup(r => r.GetAll("rock", 2, 5))
                .ReturnsAsync((expectedSongs, 10, 2));

            // Act
            var (songs, totalCount, totalPages) = await _service.GetAllAsync("rock", 2, 5);

            // Assert
            songs.Should().ContainSingle().Which.Title.Should().Be("Song1");
            totalCount.Should().Be(10);
            totalPages.Should().Be(2);
            _repoMock.Verify(r => r.GetAll("rock", 2, 5), Times.Once);
        }
    }
}