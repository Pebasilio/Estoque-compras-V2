using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiEstoqueRoupas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ApiEstoqueRoupas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        private static readonly Dictionary<string, (string Password, string Name, string Role)> Users = new()
        {
            ["admin@stockpro.com"] = ("admin123", "Administrador", "Admin"),
            ["gestor@stockpro.com"] = ("gestor123", "Gestor de Estoque", "Manager")
        };

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            if (!Users.TryGetValue(email, out var user) || user.Password != request.Password)
            {
                return Unauthorized(new { message = "E-mail ou senha invalidos." });
            }

            var expiresAt = DateTime.UtcNow.AddHours(2);
            var token = GenerateToken(email, user.Name, user.Role, expiresAt);

            return Ok(new LoginResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = new AuthUser
                {
                    Name = user.Name,
                    Email = email,
                    Role = user.Role
                }
            });
        }

        private string GenerateToken(string email, string name, string role, DateTime expiresAt)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key nao configurada.");
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
