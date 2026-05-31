using System.Threading.Tasks;
using ApiEstoqueRoupas.Models;
using ApiEstoqueRoupas.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApiEstoqueRoupas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductsController(IProductRepository repository, ICategoryRepository categoryRepository)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _repository.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product is null) return NotFound(new { message = $"Produto {id} não encontrado." });
            return Ok(product);
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock()
        {
            var products = await _repository.GetLowStockAsync();
            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
                return BadRequest(new { message = "Nome é obrigatório." });

            if (product.Quantity < 0)
                return BadRequest(new { message = "Quantidade não pode ser negativa." });

            if (product.ReorderThreshold < 0)
                return BadRequest(new { message = "Limite de reposição não pode ser negativo." });

            if (product.Price < 0)
                return BadRequest(new { message = "Preço não pode ser negativo." });

            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
            if (category is null)
                return BadRequest(new { message = $"Categoria {product.CategoryId} não existe." });

            product.Category = null;
            var created = await _repository.AddAsync(product);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            if (id != product.Id)
                return BadRequest(new { message = "ID da rota não corresponde ao ID do corpo." });

            if (string.IsNullOrWhiteSpace(product.Name))
                return BadRequest(new { message = "Nome é obrigatório." });

            if (product.Quantity < 0 || product.ReorderThreshold < 0 || product.Price < 0)
                return BadRequest(new { message = "Valores numéricos não podem ser negativos." });

            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
            if (category is null)
                return BadRequest(new { message = $"Categoria {product.CategoryId} não existe." });

            var updated = await _repository.UpdateAsync(product);
            if (!updated) return NotFound(new { message = $"Produto {id} não encontrado." });

            return Ok(new { message = "Produto atualizado com sucesso.", product });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted) return NotFound(new { message = $"Produto {id} não encontrado." });
            return Ok(new { message = "Produto removido com sucesso." });
        }
    }
}
