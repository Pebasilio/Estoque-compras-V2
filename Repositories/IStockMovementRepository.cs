using System.Collections.Generic;
using System.Threading.Tasks;
using ApiEstoqueRoupas.Models;

namespace ApiEstoqueRoupas.Repositories
{
    public interface IStockMovementRepository
    {
        Task<StockMovement> AddAsync(StockMovement movement);
        Task<List<StockMovement>> GetByProductAsync(int productId);
        Task<List<StockMovement>> GetAllAsync(MovementType? type, int take = 100);
        Task<List<StockMovement>> GetTodayAsync();
    }
}
