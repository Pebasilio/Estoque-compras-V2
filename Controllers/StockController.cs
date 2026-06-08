using System;
using System.Linq;
using System.Threading.Tasks;
using ApiEstoqueRoupas.Models;
using ApiEstoqueRoupas.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiEstoqueRoupas.Controllers
{
    // ==========================================
    // CONTROLADOR DE OPERAÇÕES DE ESTOQUE
    // Rota base: /api/stock
    // Onde a mágica (e a lógica de negócios complexa) acontece!
    // ==========================================
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IStockMovementRepository _movementRepository;

        // Injeta Produto e Movimentação, pois sempre que mexemos no estoque, mexemos nas duas tabelas
        public StockController(IProductRepository productRepository, IStockMovementRepository movementRepository)
        {
            _productRepository = productRepository;
            _movementRepository = movementRepository;
        }

        // Rota: POST /api/stock/entry
        // Dá entrada no estoque de um produto e gera o histórico
        [HttpPost("entry")]
        public async Task<IActionResult> Entry([FromBody] StockEntryRequest request)
        {
            if (request.Quantity <= 0)
                return BadRequest(new { message = "Quantidade deve ser maior que zero." });

            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product is null)
                return NotFound(new { message = "Produto não encontrado." });

            // 1) Prepara o "recibo" (histórico) da movimentação
            var movement = new StockMovement
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Type = MovementType.ENTRADA,
                Quantity = request.Quantity,
                StockBefore = product.Quantity, // Salva uma "foto" de como estava o saldo antes
                StockAfter = product.Quantity + request.Quantity, // Como vai ficar o saldo depois
                Reason = request.Reason,
                User = request.User,
                Date = DateTime.Now
            };

            // 2) Efetiva a mudança somando no saldo atual do produto
            product.Quantity += request.Quantity;
            
            // 3) Salva tudo no banco de dados
            await _productRepository.UpdateAsync(product);
            await _movementRepository.AddAsync(movement);

            return Ok(new StockMovementResponse
            {
                Success = true,
                Message = $"Entrada registrada. Novo estoque: {product.Quantity}",
                Movement = movement,
                NeedsRestock = product.Quantity <= product.ReorderThreshold,
                CurrentStock = product.Quantity,
                ReorderThreshold = product.ReorderThreshold
            });
        }

        // Rota: POST /api/stock/exit
        // Retira produtos do estoque (Ex: Venda, Avaria)
        [HttpPost("exit")]
        public async Task<IActionResult> Exit([FromBody] StockExitRequest request)
        {
            if (request.Quantity <= 0)
                return BadRequest(new { message = "Quantidade deve ser maior que zero." });

            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product is null)
                return NotFound(new { message = "Produto não encontrado." });

            // REGRA DE NEGÓCIO CRÍTICA: Travar saída se o cliente tentar tirar mais do que possui (Evita estoque negativo)
            if (product.Quantity < request.Quantity)
                return BadRequest(new { message = $"Estoque insuficiente. Disponível: {product.Quantity}" });

            // Cria o registro de movimentação de saída com os mesmos campos do registro de entrada
            var movement = new StockMovement
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Type = MovementType.SAIDA,
                Quantity = request.Quantity,
                StockBefore = product.Quantity,
                StockAfter = product.Quantity - request.Quantity,
                Reason = request.Reason,
                User = request.User,
                Date = DateTime.Now
            };

            // Subtrai a quantidade do saldo do produto
            product.Quantity -= request.Quantity;
            // Persiste as alterações no banco (atualiza produto e salva histórico)
            await _productRepository.UpdateAsync(product);
            await _movementRepository.AddAsync(movement);

            // Verifica se após a saída o estoque caiu para a zona de perigo
            var needsRestock = product.Quantity <= product.ReorderThreshold;

            // Retorna a resposta com mensagem de alerta especial caso o estoque esteja baixo
            return Ok(new StockMovementResponse
            {
                Success = true,
                Message = needsRestock
                    ? $"Saída registrada. ATENÇÃO: estoque baixo ({product.Quantity})."
                    : $"Saída registrada. Estoque: {product.Quantity}",
                Movement = movement,
                NeedsRestock = needsRestock,
                CurrentStock = product.Quantity,
                ReorderThreshold = product.ReorderThreshold
            });
        }

        // Rota: GET /api/stock/history/{productId}
        // Retorna o histórico de todas as movimentações de um produto específico (ordenado por data decrescente)
        [HttpGet("history/{productId:int}")]
        public async Task<IActionResult> History(int productId)
        {
            var movements = await _movementRepository.GetByProductAsync(productId);
            return Ok(movements);
        }

        // Rota: GET /api/stock/movements?type=ENTRADA|SAIDA
        // Retorna as últimas movimentações globais, com filtro opcional por tipo via query string
        [HttpGet("movements")]
        public async Task<IActionResult> Movements([FromQuery] string? type)
        {
            // Inicializa o filtro como nulo (sem filtro = traz tudo)
            MovementType? filter = null;
            if (!string.IsNullOrEmpty(type))
            {
                // Tenta converter o texto "ENTRADA" ou "SAIDA" para o Enum correspondente
                if (!Enum.TryParse<MovementType>(type.ToUpper(), out var parsed))
                    return BadRequest(new { message = "Tipo inválido. Use ENTRADA ou SAIDA." });
                filter = parsed;
            }

            // Busca as movimentações no banco com o filtro aplicado (ou sem filtro)
            var movements = await _movementRepository.GetAllAsync(filter);
            return Ok(movements);
        }

        // Rota: GET /api/stock/restock-alerts
        // Motor inteligente que calcula quanto o gestor precisa comprar com base no "Limiar de Reposição" (ReorderThreshold)
        [HttpGet("restock-alerts")]
        public async Task<IActionResult> RestockAlerts()
        {
            // Puxa do banco APENAS os itens que o estoque <= Limite de Risco
            var products = await _productRepository.GetLowStockAsync();

            // Mapeia e calcula a recomendação de compra para cada um
            var alerts = products.Select(p => new RestockAlert
            {
                ProductId = p.Id,
                ProductName = p.Name,
                Category = p.Category?.Name ?? string.Empty,
                CurrentStock = p.Quantity,
                ReorderThreshold = p.ReorderThreshold,
                
                // CÁLCULO DE COMPRA: Exemplo, se o Limite é 5, e eu tenho 2 no estoque:
                // (5 * 3) - 2 = 15 - 2 = Sugere comprar 13 peças! 
                // Assim o estoque sobe para 15 (Ficando confortavelmente acima da zona de risco)
                SuggestedOrderQuantity = (p.ReorderThreshold * 3) - p.Quantity,
                
                AlertLevel = p.Quantity == 0 ? "CRITICAL" : "WARNING" // Se estiver 0 absoluto, é crítico
            }).OrderBy(a => a.CurrentStock).ToList();

            return Ok(new { count = alerts.Count, alerts });
        }

        // Rota: GET /api/stock/report
        // Gera um relatório completo do estado atual do estoque (requer autenticação JWT)
        // O atributo [Authorize] obriga o usuário a enviar um token válido para acessar este endpoint
        [Authorize]
        [HttpGet("report")]
        public async Task<IActionResult> Report()
        {
            // Carrega todos os produtos e as movimentações do dia atual
            var products = await _productRepository.GetAllAsync();
            var todayMovements = await _movementRepository.GetTodayAsync();

            // === Calcula as métricas do Dashboard ===
            var totalProducts = products.Count;                                          // Total de produtos cadastrados
            var lowStockCount = products.Count(p => p.Quantity <= p.ReorderThreshold);   // Quantos estão com estoque baixo
            var outOfStockCount = products.Count(p => p.Quantity == 0);                  // Quantos estão zerados
            var totalUnits = products.Sum(p => p.Quantity);                              // Soma de todas as unidades em estoque
            var totalInventoryValue = products.Sum(p => p.TotalStockValue);              // Valor financeiro total do inventário
            var averagePrice = products.Any() ? products.Average(p => p.Price) : 0m;     // Preço médio dos produtos

            // Filtra as movimentações do dia separando entradas e saídas
            var todayEntries = todayMovements.Where(m => m.Type == MovementType.ENTRADA).Sum(m => m.Quantity);
            var todayExits = todayMovements.Where(m => m.Type == MovementType.SAIDA).Sum(m => m.Quantity);

            // Retorna um objeto anônimo com todos os indicadores para o Frontend popular o Dashboard
            return Ok(new
            {
                totalProducts,
                lowStockCount,
                outOfStockCount,
                totalUnits,
                totalInventoryValue,
                averagePrice,
                todayEntries,
                todayExits,
                lastUpdate = DateTime.Now // Marca a data/hora em que o relatório foi gerado
            });
        }
    }
}
