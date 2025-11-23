using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Dotnet_test.Infrastructure;
using Dotnet_test.Repository;
using Dotnet_test.Domain;
using Dotnet_test.DTOs.User;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Backend.Tests.Repository
{
    public class UserRepositoryTests
    {
        private static ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private static IConfiguration CreateFakeConfig()
        {
            var dict = new Dictionary<string, string>
            {
                {"Jwt:Key", "super_secret_key_1234567890_super_secret_key_1234567890"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
            };

            return new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        }

        [Fact]
        public async Task Create_ShouldAddUser_WhenEmailUnique()
        {
            await using var context = CreateContext(nameof(Create_ShouldAddUser_WhenEmailUnique));
            var repo = new UserRepository(context, CreateFakeConfig());

            var user = new User
            {
                Email = "a@test.com",
                Username = "Alpha",
                PasswordHash = "HASH"
            };

            var result = await repo.Create(user);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            (await context.Users.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task Create_ShouldThrow_WhenEmailExists()
        {
            await using var context = CreateContext(nameof(Create_ShouldThrow_WhenEmailExists));
            context.Users.Add(new User { Email = "taken@test.com", Username = "Old" });
            await context.SaveChangesAsync();

            var repo = new UserRepository(context, CreateFakeConfig());
            var newUser = new User { Email = "taken@test.com", Username = "New" };

            Func<Task> act = async () => await repo.Create(newUser);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Delete_ShouldReturnFalse_WhenUserMissing()
        {
            await using var context = CreateContext(nameof(Delete_ShouldReturnFalse_WhenUserMissing));
            var repo = new UserRepository(context, CreateFakeConfig());

            var result = await repo.Delete(99);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Delete_ShouldRemoveUserAndRelatedData()
        {
            await using var context = CreateContext(nameof(Delete_ShouldRemoveUserAndRelatedData));
            var user = new User { Username = "U", Email = "u@test.com" };
            var party = new Party { Name = "P", HostUser = user, Status = Status.Active };
            var participant = new Participant { Party = party, User = user };
            var song = new Song { Title = "Song" };
            var ps = new PlaylistSong { Song = song, AddedByUser = user };
            user.HostedParties.Add(party);
            user.PartiesJoined.Add(participant);
            user.AddedPlaylistSongs.Add(ps);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context, CreateFakeConfig());
            var deleted = await repo.Delete(user.Id);

            deleted.Should().BeTrue();
            context.Users.Should().BeEmpty();
            context.Parties.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllUsers()
        {
            await using var context = CreateContext(nameof(GetAll_ShouldReturnAllUsers));
            context.Users.AddRange(
                new User { Email = "1@test.com" },
                new User { Email = "2@test.com" }
            );
            await context.SaveChangesAsync();

            var repo = new UserRepository(context, CreateFakeConfig());
            var users = await repo.GetAll();

            users.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetById_ShouldReturnUser_WhenFound()
        {
            await using var context = CreateContext(nameof(GetById_ShouldReturnUser_WhenFound));
            var user = new User { Email = "a@test.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context, CreateFakeConfig());
            var result = await repo.GetById(user.Id);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenNotFound()
        {
            await using var context = CreateContext(nameof(GetById_ShouldReturnNull_WhenNotFound));
            var repo = new UserRepository(context, CreateFakeConfig());
            var result = await repo.GetById(10);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Update_ShouldModifyFieldsAndReturnUser()
        {
            await using var context = CreateContext(nameof(Update_ShouldModifyFieldsAndReturnUser));
            var user = new User { Username = "Old", Email = "old@test.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var dto = new UpdateUserDTO { Username = "NewName", Password = "1234", Role = "Admin" };

            var repo = new UserRepository(context, CreateFakeConfig());
            var updated = await repo.Update(user, dto);

            updated.Should().NotBeNull();
            updated!.Username.Should().Be("NewName");
            BCrypt.Net.BCrypt.Verify("1234", updated.PasswordHash).Should().BeTrue();
            updated.Role.Should().Be(Role.Admin);
        }

        [Fact]
        public async Task Update_ShouldReturnNull_WhenUserDoesNotExist()
        {
            await using var context = CreateContext(nameof(Update_ShouldReturnNull_WhenUserDoesNotExist));
            var repo = new UserRepository(context, CreateFakeConfig());
            var result = await repo.Update(new User { Id = 99 }, new UpdateUserDTO());

            result.Should().BeNull();
        }

        [Fact]
        public async Task Login_ShouldReturnNull_WhenUserNotFound()
        {
            await using var context = CreateContext(nameof(Login_ShouldReturnNull_WhenUserNotFound));
            var repo = new UserRepository(context, CreateFakeConfig());
            var dto = new LoginDTO("x@test.com", "123");

            var result = await repo.Login(dto);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Login_ShouldReturnNull_WhenPasswordInvalid()
        {
            await using var context = CreateContext(nameof(Login_ShouldReturnNull_WhenPasswordInvalid));
            var user = new User
            {
                Email = "a@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret"),
                Username = "A"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context, CreateFakeConfig());
            var dto = new LoginDTO("a@test.com", "wrong");
            var result = await repo.Login(dto);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsValid()
        {
            await using var context = CreateContext(nameof(Login_ShouldReturnToken_WhenCredentialsValid));
            var user = new User
            {
                Email = "a@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret"),
                Username = "A"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context, CreateFakeConfig());
            var dto = new LoginDTO("a@test.com", "secret");

            var result = await repo.Login(dto);

            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrEmpty();
            result.Username.Should().Be(user.Username);
        }
    }
}