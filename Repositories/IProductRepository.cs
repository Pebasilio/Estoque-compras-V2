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
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        
        // Método especializado para o Dashboard: Filtra diretamente no banco produtos que precisam de reposição
        Task<List<Product>> GetLowStockAsync();
        
        Task<Product> AddAsync(Product product);
        Task<bool> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);
        
        // Retorna apenas um booleano (true/false) para checagens rápidas se o produto existe, sem carregar seus dados na memória
        Task<bool> ExistsAsync(int id);
        
        // Método genérico para forçar um "commit" ou salvamento no banco, caso o repositório use Unit of Work (EF Core)
        Task SaveAsync();
    }
}
