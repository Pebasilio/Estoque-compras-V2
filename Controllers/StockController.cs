using System;
using System.Linq;
using System.Threading.Tasks;
using ApiEstoqueRoupas.Models;
using ApiEstoqueRoupas.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApiEstoqueRoupas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IStockMovementRepository _movementRepository;

        public StockController(IProductRepository productRepository, IStockMovementRepository movementRepository)
        {
            _productRepository = productRepository;
            _movementRepository = movementRepository;
        }

        [HttpPost("entry")]
        public async Task<IActionResult> Entry([FromBody] StockEntryRequest request)
        {
            if (request.Quantity <= 0)
                return BadRequest(new { message = "Quantidade deve ser maior que zero." });

            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product is null)
                return NotFound(new { message = "Produto não encontrado." });

            var movement = new StockMovement
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Type = MovementType.ENTRADA,
                Quantity = request.Quantity,
                StockBefore = product.Quantity,
                StockAfter = product.Quantity + request.Quantity,
                Reason = request.Reason,
                User = request.User,
                Date = DateTime.Now
            };

            product.Quantity += request.Quantity;
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

        [HttpPost("exit")]
        public async Task<IActionResult> Exit([FromBody] StockExitRequest request)
        {
            if (request.Quantity <= 0)
                return BadRequest(new { message = "Quantidade deve ser maior que zero." });

            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product is null)
                return NotFound(new { message = "Produto não encontrado." });

            if (product.Quantity < request.Quantity)
                return BadRequest(new { message = $"Estoque insuficiente. Disponível: {product.Quantity}" });

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

            product.Quantity -= request.Quantity;
            await _productRepository.UpdateAsync(product);
            await _movementRepository.AddAsync(movement);

            var needsRestock = product.Quantity <= product.ReorderThreshold;

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

        [HttpGet("history/{productId:int}")]
        public async Task<IActionResult> History(int productId)
        {
            var movements = await _movementRepository.GetByProductAsync(productId);
            return Ok(movements);
        }

        [HttpGet("movements")]
        public async Task<IActionResult> Movements([FromQuery] string? type)
        {
            MovementType? filter = null;
            if (!string.IsNullOrEmpty(type))
            {
                if (!Enum.TryParse<MovementType>(type.ToUpper(), out var parsed))
                    return BadRequest(new { message = "Tipo inválido. Use ENTRADA ou SAIDA." });
                filter = parsed;
            }

            var movements = await _movementRepository.GetAllAsync(filter);
            return Ok(movements);
        }

        [HttpGet("restock-alerts")]
        public async Task<IActionResult> RestockAlerts()
        {
            var products = await _productRepository.GetLowStockAsync();

            var alerts = products.Select(p => new RestockAlert
            {
                ProductId = p.Id,
                ProductName = p.Name,
                Category = p.Category?.Name ?? string.Empty,
                CurrentStock = p.Quantity,
                ReorderThreshold = p.ReorderThreshold,
                SuggestedOrderQuantity = (p.ReorderThreshold * 3) - p.Quantity,
                AlertLevel = p.Quantity == 0 ? "CRITICAL" : "WARNING"
            }).OrderBy(a => a.CurrentStock).ToList();

            return Ok(new { count = alerts.Count, alerts });
        }

        [HttpGet("report")]
        public async Task<IActionResult> Report()
        {
            var products = await _productRepository.GetAllAsync();
            var todayMovements = await _movementRepository.GetTodayAsync();

            var totalProducts = products.Count;
            var lowStockCount = products.Count(p => p.Quantity <= p.ReorderThreshold);
            var outOfStockCount = products.Count(p => p.Quantity == 0);
            var totalUnits = products.Sum(p => p.Quantity);
            var totalInventoryValue = products.Sum(p => p.TotalStockValue);
            var averagePrice = products.Any() ? products.Average(p => p.Price) : 0m;

            var todayEntries = todayMovements.Where(m => m.Type == MovementType.ENTRADA).Sum(m => m.Quantity);
            var todayExits = todayMovements.Where(m => m.Type == MovementType.SAIDA).Sum(m => m.Quantity);

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
                lastUpdate = DateTime.Now
            });
        }
    }
}
