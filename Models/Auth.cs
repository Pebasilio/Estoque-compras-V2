using System;

namespace ApiEstoqueRoupas.Models
{
    // ==========================================
    // DTOs DE AUTENTICAÇÃO
    // Modelos usados exclusivamente no fluxo de login (requisição e resposta)
    // ==========================================

    // DTO que representa os dados enviados pelo Frontend ao fazer login
    public class LoginRequest
    {
        // E-mail do usuário que está tentando logar
        public string Email { get; set; } = string.Empty;
        // Senha em texto plano (em produção, nunca trafegar sem HTTPS!)
        public string Password { get; set; } = string.Empty;
    }

    // Representa os dados públicos do usuário autenticado (sem a senha!)
    // Esses dados são enviados ao Frontend junto com o token para exibição na interface
    public class AuthUser
    {
        // Nome de exibição do usuário (ex: "Administrador")
        public string Name { get; set; } = string.Empty;
        // E-mail do usuário autenticado
        public string Email { get; set; } = string.Empty;
        // Papel/permissão do usuário no sistema (ex: "Admin", "Manager")
        public string Role { get; set; } = string.Empty;
    }

    // Resposta completa enviada ao Frontend após um login bem-sucedido
    public class LoginResponse
    {
        // Token JWT que o Frontend deve armazenar e enviar em cada requisição protegida (no header Authorization)
        public string Token { get; set; } = string.Empty;
        // Data/hora de expiração do token (o Frontend pode usar para fazer logout automático)
        public DateTime ExpiresAt { get; set; }
        // Dados do usuário para exibição imediata na interface (nome, email, role)
        public AuthUser User { get; set; } = new();
    }
}
