using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Dotnet_test.Infrastructure;
using Dotnet_test.Repository;
using Dotnet_test.Domain;
using Dotnet_test.DTOs.Song;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Backend.Tests.Repository
{
    public class SongRepositoryTests
    {
        private static ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllSongs_WhenNoSearch()
        {
            await using var context = CreateContext(nameof(GetAll_ShouldReturnAllSongs_WhenNoSearch));
            context.Songs.AddRange(
                new Song { Title = "A", Artist = "Alpha", Album = "One" },
                new Song { Title = "B", Artist = "Beta", Album = "Two" }
            );
            await context.SaveChangesAsync();

            var repo = new SongRepository(context);

            var (songs, total, pages) = await repo.GetAll(null, 1, 10);

            total.Should().Be(2);
            pages.Should().Be(1);
            songs.Should().HaveCount(2);
            songs.Select(s => s.Title).Should().Contain(new[] { "A", "B" });
        }

        [Fact]
        public async Task GetAll_ShouldApplySearchFilter()
        {
            await using var context = CreateContext(nameof(GetAll_ShouldApplySearchFilter));
            context.Songs.AddRange(
                new Song { Title = "Rock Song", Artist = "Rockers", Album = "Loud" },
                new Song { Title = "Pop Hit", Artist = "Popsters", Album = "Smooth" }
            );
            await context.SaveChangesAsync();

            var repo = new SongRepository(context);

            var (songs, total, pages) = await repo.GetAll("rock", 1, 10);

            songs.Should().ContainSingle(s => s.Title.Contains("Rock"));
            total.Should().Be(1);
            pages.Should().Be(1);
        }

        [Fact]
        public async Task GetAll_ShouldRespectPaging()
        {
            await using var context = CreateContext(nameof(GetAll_ShouldRespectPaging));
            for (int i = 1; i <= 25; i++)
                context.Songs.Add(new Song { Title = $"Title {i}", Artist = "Artist", Album = "Alb" });
            await context.SaveChangesAsync();

            var repo = new SongRepository(context);
            var (songs, total, pages) = await repo.GetAll(null, 2, 10);

            songs.Should().HaveCount(10);
            total.Should().Be(25);
            pages.Should().Be(3);
        }

        [Fact]
        public async Task GetById_ShouldReturnSong_WhenExists()
        {
            await using var context = CreateContext(nameof(GetById_ShouldReturnSong_WhenExists));
            var song = new Song { Title = "Song1", Artist = "Artist", Album = "A1" };
            context.Songs.Add(song);
            await context.SaveChangesAsync();

            var repo = new SongRepository(context);
            var result = await repo.GetById(song.Id);

            result.Should().NotBeNull();
            result!.Title.Should().Be("Song1");
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenMissing()
        {
            await using var context = CreateContext(nameof(GetById_ShouldReturnNull_WhenMissing));
            var repo = new SongRepository(context);
            var result = await repo.GetById(99);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Search_ShouldReturnMatchingSongs()
        {
            await using var context = CreateContext(nameof(Search_ShouldReturnMatchingSongs));
            context.Songs.AddRange(
                new Song { Title = "Hello", Artist = "Tester", Album = "One" },
                new Song { Title = "Goodbye", Artist = "Tester", Album = "Two" }
            );
            await context.SaveChangesAsync();

            var repo = new SongRepository(context);
            var result = await repo.Search("Hello", 5);

            result.Should().ContainSingle(s => s.Title == "Hello");
        }

        [Fact]
        public async Task Search_ShouldReturnEmptyList_WhenQueryNullOrEmpty()
        {
            await using var context = CreateContext(nameof(Search_ShouldReturnEmptyList_WhenQueryNullOrEmpty));
            var repo = new SongRepository(context);

            var result1 = await repo.Search("", 5);
            var result2 = await repo.Search(null!, 5);

            result1.Should().BeEmpty();
            result2.Should().BeEmpty();
        }

        [Fact]
        public async Task Search_ShouldRespectLimit()
        {
            await using var context = CreateContext(nameof(Search_ShouldRespectLimit));
            for (int i = 0; i < 10; i++)
            {
                context.Songs.Add(new Song { Title = $"T{i}", Artist = "A", Album = "B" });
            }
            await context.SaveChangesAsync();

            var repo = new SongRepository(context);
            var result = await repo.Search("t", 3);

            result.Should().HaveCount(3);
        }
    }
}