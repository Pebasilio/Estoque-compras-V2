import { fetchWithAuth, USE_MOCK_DATA } from './api';

// ==========================================
// MOCK DATA (O Plano B)
// Usado caso a flag USE_MOCK_DATA do arquivo api.js esteja ligada.
// ==========================================
const MOCK_PRODUCTS = [
  { id: 1, name: "Camiseta Algodão", categoryId: 1, category: { name: "Camisetas" }, quantity: 45, reorderThreshold: 20, price: 59.90, stockStatus: "OK" },
  { id: 2, name: "Calça Jeans Slim", categoryId: 2, category: { name: "Calças" }, quantity: 8, reorderThreshold: 15, price: 129.90, stockStatus: "ESTOQUE_BAIXO" },
  { id: 3, name: "Jaqueta Couro", categoryId: 3, category: { name: "Casacos" }, quantity: 0, reorderThreshold: 5, price: 399.90, stockStatus: "SEM_ESTOQUE" },
];

// ==========================================
// SERVIÇO DE PRODUTOS E ESTOQUE
// O elo de ligação entre as Telas de Produto/Estoque e a API C#
// ==========================================
export const produtoService = {
  // GET: Obter todos os produtos listados
  async getProdutos() {
    if (USE_MOCK_DATA) return Promise.resolve(MOCK_PRODUCTS);
    return fetchWithAuth('/products'); // O fetchWithAuth já embute o Token JWT e trata erros comuns, simplificando o código aqui
  },

  // GET: Obter produto específico
  async getProdutoById(id) {
    if (USE_MOCK_DATA) return Promise.resolve(MOCK_PRODUCTS.find(p => p.id == id));
    return fetchWithAuth(`/products/${id}`);
  },

  // GET: Produtos com estoque baixo
  async getLowStock() {
    if (USE_MOCK_DATA) return Promise.resolve(MOCK_PRODUCTS.filter(p => p.quantity <= p.reorderThreshold));
    return fetchWithAuth('/products/low-stock');
  },

  // POST: Criar novo produto
  async createProduto(produtoData) {
    if (USE_MOCK_DATA) {
      MOCK_PRODUCTS.push({ ...produtoData, id: Math.floor(Math.random() * 1000) });
      return Promise.resolve(produtoData);
    }
    return fetchWithAuth('/products', {
      method: 'POST',
      body: JSON.stringify(produtoData)
    });
  },

  // PUT: Atualizar produto inteiro
  async updateProduto(id, produtoData) {
    if (USE_MOCK_DATA) return Promise.resolve(produtoData);
    return fetchWithAuth(`/products/${id}`, {
      method: 'PUT',
      body: JSON.stringify(produtoData)
    });
  },

  // DELETE: Remover produto
  async deleteProduto(id) {
    if (USE_MOCK_DATA) return Promise.resolve(null);
    return fetchWithAuth(`/products/${id}`, {
      method: 'DELETE'
    });
  },

  // GET: Categorias
  async getCategorias() {
    if (USE_MOCK_DATA) return Promise.resolve([{id: 1, name: "Camisetas"}, {id: 2, name: "Calças"}, {id: 3, name: "Casacos"}]);
    return fetchWithAuth('/categories');
  },

  // ----------------------------------------------------
  // ROTAS DE ESTOQUE
  // ----------------------------------------------------

  // POST: Entrada de estoque
  async registraEntrada(productId, quantity, reason, user) {
    if (USE_MOCK_DATA) return Promise.resolve({ success: true });
    return fetchWithAuth('/stock/entry', {
      method: 'POST',
      body: JSON.stringify({ productId, quantity, reason, user })
    });
  },

  // POST: Saída de estoque
  async registraSaida(productId, quantity, reason, user) {
    if (USE_MOCK_DATA) return Promise.resolve({ success: true });
    return fetchWithAuth('/stock/exit', {
      method: 'POST',
      body: JSON.stringify({ productId, quantity, reason, user })
    });
  },

  // GET: Alertas de reposição
  async getAlertasReposicao() {
    if (USE_MOCK_DATA) {
      return Promise.resolve(
        MOCK_PRODUCTS.filter(p => p.quantity <= p.reorderThreshold).map(p => ({
          product: p,
          suggestedQuantity: (p.reorderThreshold * 3) - p.quantity
        }))
      );
    }
    
    // O backend retorna um objeto { count: N, alerts: [...] } 
    const response = await fetchWithAuth('/stock/restock-alerts');
    
    // Mapeando do formato do C# (RestockAlert) para o formato que o React espera
    return response.alerts.map(a => ({
      product: {
        id: a.productId,
        name: a.productName,
        quantity: a.currentStock,
        reorderThreshold: a.reorderThreshold
      },
      suggestedQuantity: a.suggestedOrderQuantity
    }));
  },

  // GET: Movimentações (Histórico)
  // Busca todas as movimentações de estoque na rota GET /api/stock/movements
  // Parâmetro opcional 'type' permite filtrar por "ENTRADA" ou "SAIDA" via query string
  async getMovements(type = null) {
    // Se estiver usando dados falsos (mock), retorna exemplos estáticos para não precisar do backend
    if (USE_MOCK_DATA) {
      return Promise.resolve([
        { id: 1, productName: "Camiseta Algodão", type: "ENTRADA", quantity: 10, reason: "Reposição", date: new Date().toISOString(), user: "Admin" },
        { id: 2, productName: "Calça Jeans Slim", type: "SAIDA", quantity: 2, reason: "Venda", date: new Date().toISOString(), user: "Vendedor" }
      ]);
    }
    // Monta a URL com ou sem filtro de tipo. Ex: /stock/movements?type=ENTRADA
    const url = type ? `/stock/movements?type=${type}` : '/stock/movements';
    return fetchWithAuth(url); // Faz a requisição autenticada com JWT
  },
  
  // PATCH (Exemplo): Atualizar apenas uma propriedade (quantidade rápida sem histórico - se suportado pelo backend)
  async patchProdutoQuantity(id, quantity) {
    if (USE_MOCK_DATA) return Promise.resolve({ success: true });
    return fetchWithAuth(`/products/${id}`, {
      method: 'PATCH',
      body: JSON.stringify({ quantity })
    });
  }
};
