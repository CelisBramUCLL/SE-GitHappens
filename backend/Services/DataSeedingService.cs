using Dotnet_test.Domain;
using Dotnet_test.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Dotnet_test.Services
{
    public class DataSeedingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataSeedingService> _logger;

        public DataSeedingService(ApplicationDbContext context, ILogger<DataSeedingService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SeedSongsFromCsvAsync(string csvFilePath)
        {
            try
            {
                _logger.LogInformation(
                    "Starting to seed songs from CSV file: {FilePath}",
                    csvFilePath
                );

                if (!File.Exists(csvFilePath))
                {
                    _logger.LogWarning("CSV file not found: {FilePath}", csvFilePath);
                    return;
                }

                // Check if songs already exist to avoid duplicates
                var existingSongsCount = await _context.Songs.CountAsync();
                if (existingSongsCount > 0)
                {
                    _logger.LogInformation("Songs already exist in database. Skipping seeding.");
                    return;
                }

                var songsAdded = 0;

                // Use FileStream with StreamReader for efficient memory usage
                using var fileStream = new FileStream(csvFilePath, FileMode.Open, FileAccess.Read);
                using var streamReader = new StreamReader(fileStream);

                // Skip header line
                await streamReader.ReadLineAsync();

                // Process file line by line using stream
                string? line;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        var song = ParseCsvLine(line);
                        if (song != null)
                        {
                            _context.Songs.Add(song);
                            songsAdded++;

                            // Save in batches to improve performance
                            if (songsAdded % 100 == 0)
                            {
                                await _context.SaveChangesAsync();
                                _logger.LogInformation("Processed {Count} songs...", songsAdded);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing CSV line: {Line}", line);
                    }
                }

                // Save any remaining songs
                if (songsAdded % 100 != 0)
                {
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Successfully seeded {Count} songs from CSV", songsAdded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding songs from CSV");
                throw;
            }
        }

        private Song? ParseCsvLine(string csvLine)
        {
            try
            {
                // Simple CSV parsing
                var fields = ParseCsvFields(csvLine);

                if (fields.Length < 5)
                    return null;

                var title = fields[0].Trim('"');
                var artist = fields[1].Trim('"');
                var album = fields[2].Trim('"');
                var durationSeconds = int.Parse(fields[3]);
                var filePath = fields[4].Trim('"');

                return new Song
                {
                    Title = title,
                    Artist = artist,
                    Album = album,
                    Duration = new Duration(0, durationSeconds), // Convert seconds to Duration struct
                    FilePath = filePath,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing CSV line: {Line}", csvLine);
                return null;
            }
        }

        private string[] ParseCsvFields(string csvLine)
        {
            var fields = new List<string>();
            var inQuotes = false;
            var currentField = "";

            for (int i = 0; i < csvLine.Length; i++)
            {
                char c = csvLine[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }

            fields.Add(currentField);
            return fields.ToArray();
        }
    }
}
