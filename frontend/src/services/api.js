// ==========================================
// CLIENTE HTTP PRINCIPAL (O Mensageiro)
// ==========================================

// This flag allows running the frontend with mock data when the API is not available
// Set to true to bypass backend requests and return hardcoded mock responses for demonstration.
export const USE_MOCK_DATA = false;

// Descobre o endereço da API C# (Usa .env se existir, senão assume a porta padrão 5123)
export const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5123/api';

/**
 * fetchWithAuth: Um "wrapper" super inteligente em volta do fetch original do navegador.
 * Por que usamos ele?
 * 1. Ele anexa automaticamente o Token de Segurança (JWT) em todas as requisições
 * 2. Ele transforma a resposta de texto direto para JSON
 * 3. Ele expulsa o usuário automaticamente para a tela de Login se o Token expirar!
 */
export async function fetchWithAuth(endpoint, options = {}) {
  // Busca o crachá do usuário no cache
  const token = localStorage.getItem('token');
  
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers, // Mantém os headers originais que alguém enviou
  };

  // Se tem token, avisa o C# que somos nós enviando o "Bearer"
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  // Dispara a requisição de rede para a URL do C#
  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    if (response.status === 401 || response.status === 403) {
      // Auto logout if token expires or is invalid
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    
    let errorData;
    try {
      errorData = await response.json();
    } catch {
      errorData = { message: 'Erro desconhecido na API' };
    }
    
    throw new Error(errorData.message || `Erro ${response.status}: Falha na requisição`);
  }

  // Some DELETE requests might not return a body
  if (response.status === 204) {
    return null;
  }

  return response.json();
}
