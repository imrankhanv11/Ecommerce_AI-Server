using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using EcommerceAI.Contracts.DTOs.User;
using EcommerceAI.Models.Entities;
using EcommerceAI.Repositories.Interfaces;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public UserService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<UserResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
            throw new ArgumentException("Email is already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower().Trim(),
            PasswordHash = HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = "Customer"
        };

        await _userRepository.CreateAsync(user);
        return MapToDto(user);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLower().Trim());
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = GenerateJwtToken(user);

        return new LoginResponseDto
        {
            Token = token,
            User = MapToDto(user)
        };
    }

    public async Task<UserResponseDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserResponseDto?> UpdateAsync(Guid id, UpdateUserRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.Email != null)
        {
            var normalizedEmail = request.Email.ToLower().Trim();
            if (normalizedEmail != user.Email && await _userRepository.EmailExistsAsync(normalizedEmail))
                throw new ArgumentException("Email is already in use.");
            user.Email = normalizedEmail;
        }

        await _userRepository.UpdateAsync(user);
        return MapToDto(user);
    }

    public async Task<UserResponseDto?> UpdateProfileAsync(Guid id, string firstName, string lastName)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        user.FirstName = firstName.Trim();
        user.LastName = lastName.Trim();

        await _userRepository.UpdateAsync(user);
        return MapToDto(user);
    }

    // ─── Password hashing using PBKDF2 ─────────────────────
    private static string HashPassword(string password)
    {
        using var hmac = new HMACSHA512();
        var salt = hmac.Key;
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var storedHashBytes = Convert.FromBase64String(parts[1]);

        using var hmac = new HMACSHA512(salt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(storedHashBytes);
    }

    // ─── JWT Token Generation ────────────────────────────────
    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "SuperSecretKeyForDevelopmentOnly12345!"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim())
        };

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"] ?? "EcommerceAI",
            audience: jwtSettings["Audience"] ?? "EcommerceAI",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserResponseDto MapToDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }
}
