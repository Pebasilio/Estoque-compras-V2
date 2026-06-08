using System.Collections.Generic;
using System.Threading.Tasks;
using ApiEstoqueRoupas.Models;

namespace ApiEstoqueRoupas.Repositories
{
    // ==========================================
    // INTERFACE DO REPOSITÓRIO DE PRODUTOS
    // ==========================================
    public interface IProductRepository
    {
        // Retorna todos os produtos cadastrados, incluindo dados da categoria vinculada
        Task<List<Product>> GetAllAsync();
        // Busca um produto pelo ID. Retorna null se o produto não existir
        Task<Product?> GetByIdAsync(int id);
        
        // Método especializado para o Dashboard: Filtra diretamente no banco produtos que precisam de reposição
        Task<List<Product>> GetLowStockAsync();
        
        // Insere um novo produto no banco e retorna o objeto com o ID gerado
        Task<Product> AddAsync(Product product);
        // Atualiza os dados de um produto existente. Retorna true se conseguiu atualizar
        Task<bool> UpdateAsync(Product product);
        // Atualiza parcialmente um produto (apenas os campos informados). Retorna null se não encontrado
        Task<Product?> PatchAsync(int id, Models.ProductPatchRequest patch);
        // Remove um produto pelo ID. Retorna true se o produto foi encontrado e excluído
        Task<bool> DeleteAsync(int id);
        
        // Retorna apenas um booleano (true/false) para checagens rápidas se o produto existe, sem carregar seus dados na memória
        Task<bool> ExistsAsync(int id);
        
        // Método genérico para forçar um "commit" ou salvamento no banco, caso o repositório use Unit of Work (EF Core)
        Task SaveAsync();
    }
}