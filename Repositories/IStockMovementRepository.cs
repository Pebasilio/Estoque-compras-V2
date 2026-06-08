using System.Collections.Generic;
using System.Threading.Tasks;
using ApiEstoqueRoupas.Models;

namespace ApiEstoqueRoupas.Repositories
{
    // ==========================================
    // INTERFACE DO REPOSITÓRIO DE MOVIMENTAÇÕES DE ESTOQUE
    // ==========================================
    public interface IStockMovementRepository
    {
        // Registra de forma permanente (histórico inalterável) uma movimentação (Entrada/Saída)
        Task<StockMovement> AddAsync(StockMovement movement);
        
        // Filtra o histórico focado apenas em um produto específico
        Task<List<StockMovement>> GetByProductAsync(int productId);
        
        // Busca o extrato de movimentações globais. 
        // Recebe um parâmetro "type" opcional para buscar só as de "ENTRADA" ou "SAIDA", limitando a no máximo 100 resultados por padrão
        Task<List<StockMovement>> GetAllAsync(MovementType? type, int take = 100);
        
        // Ferramenta analítica focada em pegar apenas a movimentação que ocorreu no dia de hoje (para preencher o Dashboard)
        Task<List<StockMovement>> GetTodayAsync();
    }
}
