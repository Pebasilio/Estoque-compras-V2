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
    // ==========================================
    // CONTROLADOR DE AUTENTICAÇÃO
    // Responsável pelo login e geração de tokens JWT.
    // Rota base: /api/auth
    // ==========================================
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Acesso às configurações do appsettings.json (chave JWT, issuer, audience)
        private readonly IConfiguration _configuration;

        // "Banco de dados" simplificado de usuários em memória (hardcoded)
        // Em produção, os usuários e senhas deveriam estar em um banco real, com senhas criptografadas (hash + salt)
        // Formato: email => (senha, nome de exibição, papel/permissão)
        private static readonly Dictionary<string, (string Password, string Name, string Role)> Users = new()
        {
            ["admin@stockpro.com"] = ("admin123", "Administrador", "Admin"),
            ["gestor@stockpro.com"] = ("gestor123", "Gestor de Estoque", "Manager")
        };

        // O ASP.NET Core injeta automaticamente a configuração (IConfiguration) graças à Injeção de Dependência
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Rota: POST /api/auth/login
        // Recebe email e senha, valida as credenciais e retorna um token JWT se forem válidas
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Normaliza o e-mail: remove espaços e converte para minúsculas para evitar diferenças de digitação
            var email = request.Email.Trim().ToLowerInvariant();

            // Tenta buscar o usuário pelo e-mail E verifica se a senha confere
            // Se falhar em qualquer uma das condições, retorna 401 (Não Autorizado)
            if (!Users.TryGetValue(email, out var user) || user.Password != request.Password)
            {
                return Unauthorized(new { message = "E-mail ou senha invalidos." });
            }

            // Define que o token expira em 2 horas a partir de agora (horário UTC)
            var expiresAt = DateTime.UtcNow.AddHours(2);
            // Gera o token JWT com as informações do usuário embutidas (claims)
            var token = GenerateToken(email, user.Name, user.Role, expiresAt);

            // Retorna 200 OK com o token, data de expiração e dados do usuário para o Frontend armazenar
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

        // Método privado responsável por montar e assinar o token JWT
        // O token é como um "documento criptografado" que carrega informações do usuário (claims)
        private string GenerateToken(string email, string name, string role, DateTime expiresAt)
        {
            // Lê a chave secreta do arquivo de configuração
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key nao configurada.");
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            // Converte a chave para o formato binário que o algoritmo de criptografia precisa
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            // Define o algoritmo HMAC-SHA256 para assinar o token (garante que ninguém adulterou os dados)
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            // "Claims" são as informações embutidas dentro do token (como campos de um crachá)
            // O Frontend pode decodificar o token e ler esses dados sem precisar chamar a API novamente
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),   // Subject: identifica quem é o dono do token
                new Claim(JwtRegisteredClaimNames.Email, email), // E-mail do usuário
                new Claim(ClaimTypes.Name, name),                // Nome de exibição
                new Claim(ClaimTypes.Role, role)                 // Papel/permissão (Admin, Manager)
            };

            // Monta o objeto do token com todas as configurações e claims definidas acima
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            // Serializa o token para uma string compacta (formato "xxxxx.yyyyy.zzzzz") que o Frontend vai armazenar
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
