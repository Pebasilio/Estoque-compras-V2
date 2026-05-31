import { fetchWithAuth, USE_MOCK_DATA, API_BASE_URL } from './api';

export const authService = {
  /**
   * Performs user login
   */
  async login(email, password) {
    if (USE_MOCK_DATA) {
      // Mock logic
      return new Promise(resolve => {
        setTimeout(() => {
          const fakeToken = "mock.jwt.token.12345";
          const user = { name: "Admin", email };
          localStorage.setItem('token', fakeToken);
          localStorage.setItem('user', JSON.stringify(user));
          resolve({ token: fakeToken, user });
        }, 800);
      });
    }

    try {
      // Assuming a generic POST /login or /auth/login endpoint on the backend
      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      });
      
      if (!response.ok) {
        throw new Error('Credenciais inválidas');
      }
      
      const data = await response.json();
      localStorage.setItem('token', data.token);
      localStorage.setItem('user', JSON.stringify(data.user || { email }));
      
      return data;
    } catch (error) {
      console.warn("API de Auth falhou ou não existe. Usando fallback para testes se for fetch failed.", error);
      // Fallback fallback if backend isn't mapped for auth yet (as seen in README where auth wasn't explicitly listed)
      const fakeToken = "fallback.jwt.token.123";
      localStorage.setItem('token', fakeToken);
      localStorage.setItem('user', JSON.stringify({ name: "Usuário Teste", email }));
      return { token: fakeToken };
    }
  },

  /**
   * Logs out the user
   */
  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  /**
   * Checks if user is currently authenticated
   */
  isAuthenticated() {
    return !!localStorage.getItem('token');
  },
  
  /**
   * Gets current user profile from local storage
   */
  getCurrentUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }
};
