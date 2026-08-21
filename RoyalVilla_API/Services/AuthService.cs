using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RoyalVilla_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        public AuthService(ApplicationDbContext db, IConfiguration configuration, IMapper mapper)
        {
            _db = db;
            _configuration = configuration;
            _mapper = mapper;
        }
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            var normalizedEmail = email.ToLower();
            return await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        }

        public async Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == loginRequestDTO.Email.ToLower());
                if (user == null || user.Password != loginRequestDTO.Password)
                {
                    return null;
                }
                // Generate token
                var token = GenerateJwtToken(user);

                return new LoginResponseDTO
                {
                    UserDTO = _mapper.Map<UserDTO>(user),
                    Token = token
                };
            }
            catch (Exception ex)
            {
                //Handle any other unexpected errors that may occur during the registration process
                throw new InvalidOperationException($"An unexpected error occurred during login: {ex.Message}", ex);
            }
        }

        public async Task<UserDTO?> RegisterAsync(RegisterationRequestDTO registerationRequestDTO)
        {
            try
            {
                if (await IsEmailExistsAsync(registerationRequestDTO.Email))
                {
                    return null;
                }

                User user = new()
                {
                    Name = registerationRequestDTO.Name,
                    Email = registerationRequestDTO.Email,
                    Password = registerationRequestDTO.Password,
                    Role = string.IsNullOrEmpty(registerationRequestDTO.Role) ? "Customer" : registerationRequestDTO.Role,
                    CreatedDate = DateTime.Now,
                };
                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();

                return _mapper.Map<UserDTO>(user);
            }
            catch (Exception ex)
            {
                //Handle any other unexpected errors that may occur during the registration process
                throw new InvalidOperationException($"An unexpected error occurred during registration: {ex.Message}", ex);
            }
        }

        private string GenerateJwtToken(User user)
        {

            var secret = _configuration.GetSection("JwtSettings")["Secret"]
                ?? throw new InvalidOperationException("JWT secret is not configured.");
            var key = System.Text.Encoding.ASCII.GetBytes(secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role)
                    }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

