using System.Collections.Generic;
using System.Threading.Tasks;
using ApiEstoqueRoupas.Models;

namespace ApiEstoqueRoupas.Repositories
{
    // ==========================================
    // INTERFACE DE REPOSITÓRIO (Contrato)
    // Usar interfaces ajuda muito em testes (Mocks) e na injeção de dependência.
    // Ela diz *O QUE* a classe deve fazer, mas não *COMO* ela faz.
    // ==========================================
    public interface ICategoryRepository
    {
        // Retorna todas as categorias em forma de lista assíncrona
        Task<List<Category>> GetAllAsync();
        
        // Busca uma categoria específica pelo ID. Pode retornar null se não existir (indicado pelo ?)
        Task<Category?> GetByIdAsync(int id);
        
        // Salva uma nova categoria no banco
        Task<Category> AddAsync(Category category);
        
        // Atualiza os dados de uma categoria (retorna true se deu certo, false se a categoria não existia)
        Task<bool> UpdateAsync(Category category);
        
        // Remove uma categoria do banco através do seu ID
        Task<bool> DeleteAsync(int id);
    }
}
