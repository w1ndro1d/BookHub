using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryAPI.Services
{
    public class AuthenticationService
    {
        private readonly LibraryDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationService(LibraryDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<Member> RegisterAsync(string fullName, string email, string password)
        {
            if (_context.Members.Any(x => x.Email == email))
                throw new Exception("Email already registered!");

            var member = new Member
            {
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                MembershipId = GenerateMembershipId()

            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return member;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var member = _context.Members.FirstOrDefault(x => x.Email == email);
            if (member == null || !BCrypt.Net.BCrypt.Verify(password, member.PasswordHash))
                throw new Exception("Invalid credentials!");

            return GenerateJwtToken(member);
        }

        //8-digit unique GUID for each member
        public string GenerateMembershipId()
        {
            return Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }

        private string GenerateJwtToken(Member member)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, member.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, member.Email),
                new Claim(ClaimTypes.Name, member.FullName),
                new Claim(ClaimTypes.Role, member.IsAdmin ? "Admin" : "Member")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds

            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
