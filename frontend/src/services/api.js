// This flag allows running the frontend with mock data when the API is not available
// Set to true to bypass backend requests and return hardcoded mock responses for demonstration.
export const USE_MOCK_DATA = false;

export const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5123/api';

/**
 * Core wrapper for fetch API
 * Automatically injects the JWT token and parses JSON
 */
export async function fetchWithAuth(endpoint, options = {}) {
  const token = localStorage.getItem('token');
  
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers,
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

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
