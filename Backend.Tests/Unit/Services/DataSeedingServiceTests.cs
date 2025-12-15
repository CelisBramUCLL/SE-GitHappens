using Dotnet_test.Domain;
using Dotnet_test.Infrastructure;
using Dotnet_test.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.IO;
using System.Threading.Tasks;

namespace Backend.Tests.Unit.Services
{
    public class DataSeedingServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly Mock<ILogger<DataSeedingService>> _loggerMock;

        public DataSeedingServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "SeedSongsDb_" + Guid.NewGuid())
                .Options;

            _loggerMock = new Mock<ILogger<DataSeedingService>>();
        }

        [Fact]
        public async Task SeedSongsFromCsvAsync_ShouldLogWarning_WhenFileDoesNotExist()
        {
            using var context = new ApplicationDbContext(_dbOptions);
            var service = new DataSeedingService(context, _loggerMock.Object);

            await service.SeedSongsFromCsvAsync("nonexistent.csv");

            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task SeedSongsFromCsvAsync_ShouldSkip_WhenSongsAlreadyExist()
        {
            using var context = new ApplicationDbContext(_dbOptions);
            context.Songs.Add(new Song { Title = "Exists" });
            await context.SaveChangesAsync();

            var service = new DataSeedingService(context, _loggerMock.Object);

            // Create any dummy csv path (it won't be read)
            var csvPath = Path.GetTempFileName();
            await File.WriteAllTextAsync(csvPath, "header\n\"Song\",\"Artist\",\"Album\",123,\"path.mp3\"");

            await service.SeedSongsFromCsvAsync(csvPath);

            var countAfter = await context.Songs.CountAsync();
            Assert.Equal(1, countAfter); // no new added
        }

        [Fact]
        public async Task SeedSongsFromCsvAsync_ShouldAddSongs_WhenValidCsv()
        {
            using var context = new ApplicationDbContext(_dbOptions);
            var service = new DataSeedingService(context, _loggerMock.Object);

            var csvPath = Path.GetTempFileName();
            await File.WriteAllTextAsync(csvPath,
                "Title,Artist,Album,Duration,File\n" +
                "\"Song1\",\"Artist1\",\"Album1\",180,\"/path1.mp3\"\n" +
                "\"Song2\",\"Artist2\",\"Album2\",200,\"/path2.mp3\"");

            await service.SeedSongsFromCsvAsync(csvPath);

            var songs = await context.Songs.ToListAsync();
            Assert.Equal(2, songs.Count);
            Assert.Contains(songs, s => s.Title == "Song1");
        }

        [Fact]
        public async Task SeedSongsFromCsvAsync_ShouldHandleInvalidLine_Safely()
        {
            using var context = new ApplicationDbContext(_dbOptions);
            var service = new DataSeedingService(context, _loggerMock.Object);

            var csvPath = Path.GetTempFileName();
            await File.WriteAllTextAsync(csvPath,
                "Title,Artist,Album,Duration,File\n" +
                "invalid_line_without_enough_fields\n");

            await service.SeedSongsFromCsvAsync(csvPath);

            var count = await context.Songs.CountAsync();
            Assert.Equal(0, count);
        }

[Fact]
public async Task SeedSongsFromCsvAsync_ShouldRethrow_WhenSaveFails()
{
    // create a stub context derived from ApplicationDbContext
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: "FailSaveDb_" + Guid.NewGuid())
        .Options;

    var context = new FailingSaveContext(options);
    var service = new DataSeedingService(context, _loggerMock.Object);

    var csvPath = Path.GetTempFileName();
    await File.WriteAllTextAsync(csvPath,
        "Title,Artist,Album,Duration,File\n" +
        "\"A\",\"B\",\"C\",100,\"p.mp3\"");

    await Assert.ThrowsAsync<InvalidOperationException>(
        () => service.SeedSongsFromCsvAsync(csvPath));
}

// helper subclass that throws on SaveChangesAsync
private class FailingSaveContext : ApplicationDbContext
{
    public FailingSaveContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("failed");
    }
}
    }
}
