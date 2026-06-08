import { USE_MOCK_DATA, API_BASE_URL } from './api';

// ==========================================
// SERVICO DE AUTENTICACAO
// Lida com Login, Logout e verificacao de quem esta logado.
// ==========================================
export const authService = {
  async login(email, password) {
    if (USE_MOCK_DATA) {
      return new Promise(resolve => {
        setTimeout(() => {
          const fakeToken = 'mock.jwt.token.12345';
          const user = { name: 'Admin', email };
          localStorage.setItem('token', fakeToken);
          localStorage.setItem('user', JSON.stringify(user));
          resolve({ token: fakeToken, user });
        }, 800);
      });
    }

    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });

    if (!response.ok) {
      let errorData;
      try {
        errorData = await response.json();
      } catch {
        errorData = { message: 'Credenciais invalidas' };
      }

      throw new Error(errorData.message || 'Credenciais invalidas');
    }

    const data = await response.json();
    localStorage.setItem('token', data.token);
    localStorage.setItem('user', JSON.stringify(data.user || { email }));

    return data;
  },

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  isAuthenticated() {
    return !!localStorage.getItem('token');
  },

  getCurrentUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }
};
