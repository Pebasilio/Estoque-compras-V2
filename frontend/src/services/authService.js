import { USE_MOCK_DATA, API_BASE_URL } from './api';

// ==========================================
// SERVICO DE AUTENTICACAO
// Lida com Login, Logout e verificacao de quem esta logado.
// ==========================================
export const authService = {
  /**
   * login: Envia email e senha para a API e, se aprovado, guarda o token JWT no navegador.
   * Esse token funciona como um "crachá digital" que prova que o usuário está logado.
   */
  async login(email, password) {
    // Se estiver no modo mock (sem backend), simula um login com dados fictícios e delay de 800ms
    if (USE_MOCK_DATA) {
      return new Promise(resolve => {
        setTimeout(() => {
          const fakeToken = 'mock.jwt.token.12345';
          const user = { name: 'Admin', email };
          localStorage.setItem('token', fakeToken); // Salva o token falso no cache do navegador
          localStorage.setItem('user', JSON.stringify(user)); // Salva os dados do usuário como JSON
          resolve({ token: fakeToken, user });
        }, 800); // Simula o tempo de resposta de uma rede real
      });
    }

    // Faz a requisição POST real para a rota de login do C# com email e senha no corpo
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });

    // Se a API devolveu erro (401, 400, etc.), tenta extrair a mensagem de erro do JSON
    if (!response.ok) {
      let errorData;
      try {
        errorData = await response.json(); // Tenta ler o corpo do erro
      } catch {
        errorData = { message: 'Credenciais invalidas' }; // Fallback se o corpo não for JSON
      }

      throw new Error(errorData.message || 'Credenciais invalidas');
    }

    // Login deu certo! Extrai os dados e guarda no localStorage para uso futuro
    const data = await response.json();
    localStorage.setItem('token', data.token); // Salva o JWT para autenticar requisições futuras
    localStorage.setItem('user', JSON.stringify(data.user || { email })); // Salva dados do perfil

    return data;
  },

  /**
   * logout: Remove completamente o "crachá" (token) e os dados do usuário do navegador.
   * Após isso, qualquer requisição autenticada será negada pela API.
   */
  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  /**
   * isAuthenticated: Verifica se existe um token salvo no navegador.
   * O "!!" converte o valor para booleano (string → true, null → false).
   * Usado pelo ProtectedRoute para decidir se deixa ou bloqueia o acesso.
   */
  isAuthenticated() {
    return !!localStorage.getItem('token');
  },

  /**
   * getCurrentUser: Recupera o objeto do usuário logado que foi salvo como JSON no login.
   * Se não houver ninguém logado, retorna null.
   * Usado pelo Navbar para exibir o nome do usuário no canto superior.
   */
  getCurrentUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null; // Converte a string JSON de volta para objeto
  }
};
