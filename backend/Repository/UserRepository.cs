using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dotnet_test.Domain;
using Dotnet_test.DTOs.User;
using Dotnet_test.Infrastructure;
using Dotnet_test.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Dotnet_test.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<User> Create(User user)
        {
            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
                throw new InvalidOperationException(
                    "A user with this email address already exists"
                );

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> Delete(int id)
        {
            // Include all relevant navigation properties
            var user = await _context
                .Users.Include(u => u.PartiesJoined)
                .Include(u => u.HostedParties)
                .ThenInclude(s => s.Participants)
                .Include(u => u.AddedPlaylistSongs)
                .ThenInclude(p => p.Song)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return false;

            // Remove participant entries where the user is just a participant
            if (user.PartiesJoined != null && user.PartiesJoined.Any())
            {
                _context.Participants.RemoveRange(user.PartiesJoined);
            }

            // Delete hosted parties (participants cascade)
            if (user.HostedParties != null && user.HostedParties.Any())
            {
                _context.Parties.RemoveRange(user.HostedParties);
            }

            // Remove playlist songs the user added
            if (user.AddedPlaylistSongs != null && user.AddedPlaylistSongs.Any())
            {
                _context.PlaylistSongs.RemoveRange(user.AddedPlaylistSongs);
            }

            // Finally delete the user
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<User>> GetAll()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetById(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User?> Update(User user, UpdateUserDTO request)
        {
            var userInDb = await _context.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
            if (userInDb == null)
            {
                return null;
            }

            if (request.FirstName != null)
                userInDb.FirstName = request.FirstName;
            if (request.LastName != null)
                userInDb.LastName = request.LastName;
            if (request.Email != null)
                userInDb.Email = request.Email;
            if (request.Password != null)
                userInDb.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            if (request.Username != null)
                userInDb.Username = request.Username;

            if (request.Role != null && Enum.TryParse<Role>(request.Role, out var parsedRole))
            {
                userInDb.Role = parsedRole;
            }

            await _context.SaveChangesAsync();
            return userInDb;
        }

        public async Task<LoginResponseDTO?> Login(LoginDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return null;

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return new LoginResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
                Token = token,
            };
        }

        // JWT token generation method
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
