using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text; // Fixes CS0103: Encoding
using Dotnet_test.Domain;
using Dotnet_test.DTOs.Product;
using Dotnet_test.DTOs.User;
using Dotnet_test.Infrastructure;
using Dotnet_test.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // Needed for IConfiguration
using Microsoft.IdentityModel.Tokens; // Fixes CS0246: SymmetricSecurityKey

namespace Dotnet_test.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration; // Fixes CS0103: _configuration

        public UserRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<User> Create(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> Delete(int id)
        {
            // Include all relevant navigation properties
            var user = await _context
                .Users.Include(u => u.SessionsJoined) // participant entries
                .Include(u => u.HostedSessions)
                .ThenInclude(s => s.Participants) // participants in hosted sessions
                .Include(u => u.AddedPlaylistSongs) // songs user added to playlists
                .ThenInclude(p => p.Song) // songs in hosted playlists
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return false;

            // 1️⃣ Remove participant entries where the user is just a participant
            if (user.SessionsJoined != null && user.SessionsJoined.Any())
            {
                _context.Participants.RemoveRange(user.SessionsJoined);
            }

            // 2️⃣ Delete hosted sessions (participants cascade)
            if (user.HostedSessions != null && user.HostedSessions.Any())
            {
                _context.Sessions.RemoveRange(user.HostedSessions);
            }

            // 3️⃣ Remove playlist songs the user added
            if (user.AddedPlaylistSongs != null && user.AddedPlaylistSongs.Any())
            {
                _context.PlaylistSongs.RemoveRange(user.AddedPlaylistSongs);
            }

            // 5️⃣ Finally delete the user
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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null)
                return null;

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            // Generate JWT
            var token = GenerateJwtToken(user);

            return new LoginResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
                Token = token,
            };
        }

        // Helper for JWT
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
