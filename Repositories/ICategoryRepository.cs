using System.Collections.Generic;
using System.Threading.Tasks;
using ApiEstoqueRoupas.Models;

namespace ApiEstoqueRoupas.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category> AddAsync(Category category);
        Task<bool> UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id);
    }
}
