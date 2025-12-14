using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Dotnet_test.DTOs.Party;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Integration.Controllers
{
    public class PartyControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public PartyControllerTests(CustomWebApplicationFactory factory)
        {
      _client = factory
    .WithWebHostBuilder(builder =>
    {
        builder.ConfigureServices(services =>
        {
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    })
    .CreateClient();
        }

        [Fact]
        public async Task GetAll_ShouldReturnEmptyList_WhenNoPartiesExist()
        {
            var response = await _client.GetAsync("/api/party");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var parties = await response.Content.ReadFromJsonAsync<IEnumerable<PartyDTO>>();
            parties.Should().NotBeNull();
            parties.Should().BeEmpty();
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenPartyDoesNotExist()
        {
            var response = await _client.GetAsync("/api/party/9999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateParty_ShouldReturnCreatedParty()
        {
            // Arrange
            var dto = new CreatePartyDTO("IntegrationTest Party");

            // You might mock user authentication or skip Auth temporarily for test
            var response = await _client.PostAsJsonAsync("/api/party", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var party = await response.Content.ReadFromJsonAsync<PartyDTO>();
            party!.Name.Should().Be("IntegrationTest Party");
        }
    }
}