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

  // Se a resposta HTTP não foi bem-sucedida (status fora do range 200-299)
  if (!response.ok) {
    if (response.status === 401 || response.status === 403) {
      // Logout automático: se o token expirou ou é inválido, limpa tudo e joga pro login
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    
    // Tenta extrair a mensagem de erro do corpo da resposta (JSON)
    let errorData;
    try {
      errorData = await response.json();
    } catch {
      errorData = { message: 'Erro desconhecido na API' }; // Fallback se o corpo não for JSON
    }
    
    // Lança uma exceção para que o código chamador (service) possa tratar no catch
    throw new Error(errorData.message || `Erro ${response.status}: Falha na requisição`);
  }

  // Requisições DELETE geralmente retornam 204 (No Content) sem corpo na resposta
  if (response.status === 204) {
    return null;
  }

  // Para todas as outras respostas bem-sucedidas, converte o corpo de texto para JSON
  return response.json();
}
