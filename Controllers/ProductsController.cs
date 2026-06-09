using System.Threading.Tasks;
using ApiEstoqueRoupas.Models;
using ApiEstoqueRoupas.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApiEstoqueRoupas.Controllers
{
    // ==========================================
    // CONTROLADOR DE PRODUTOS
    // Rota base: /api/products
    // ==========================================
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        // Repositório de produtos (operações CRUD)
        private readonly IProductRepository _repository;
        // Repositório de categorias (usado para validar se a categoria informada existe)
        private readonly ICategoryRepository _categoryRepository;

        // Injeta os dois repositórios, pois para criar um produto, precisamos verificar se a categoria dele realmente existe
        public ProductsController(IProductRepository repository, ICategoryRepository categoryRepository)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
        }

        // Rota: GET /api/products
        // Retorna a lista completa de todos os produtos cadastrados, incluindo dados da categoria
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _repository.GetAllAsync();
            return Ok(products);
        }

        // Rota: GET /api/products/{id}
        // Busca um produto específico pelo seu ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            // Retorna 404 se o produto não existir, ou 200 OK com os dados completos
            if (product is null) return NotFound(new { message = $"Produto {id} não encontrado." });
            return Ok(product);
        }

        // Rota: GET /api/products/low-stock
        // Retorna apenas os produtos cujo estoque está abaixo ou igual ao limite de reposição (ReorderThreshold)
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock()
        {
            var products = await _repository.GetLowStockAsync();
            return Ok(products);
        }

        // Rota: POST /api/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            // Validações pesadas de Segurança e Negócio antes de chegar no Banco
            if (string.IsNullOrWhiteSpace(product.Name))
                return BadRequest(new { message = "Nome é obrigatório." });

            if (product.Quantity < 0)
                return BadRequest(new { message = "Quantidade não pode ser negativa." });

            if (product.ReorderThreshold < 0)
                return BadRequest(new { message = "Limite de reposição não pode ser negativo." });

            if (product.Price < 0)
                return BadRequest(new { message = "Preço não pode ser negativo." });

            // Verifica integridade referencial: a categoria escolhida no frontend existe no banco?
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
            if (category is null)
                return BadRequest(new { message = $"Categoria {product.CategoryId} não existe." });

            // Evita salvar um objeto inteiro aninhado acidentalmente enviado pelo Frontend
            product.Category = null;
            var created = await _repository.AddAsync(product);
            
            // Retorna 201 Created apontando para a rota de busca por ID
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // Rota: PUT /api/products/{id}
        // Atualiza todos os campos de um produto existente
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            // Garante que o ID enviado na URL corresponde ao ID do objeto no corpo da requisição
            if (id != product.Id)
                return BadRequest(new { message = "ID da rota não corresponde ao ID do corpo." });

            if (string.IsNullOrWhiteSpace(product.Name))
                return BadRequest(new { message = "Nome é obrigatório." });

            // Validação simplificada: nenhum valor numérico pode ser negativo
            if (product.Quantity < 0 || product.ReorderThreshold < 0 || product.Price < 0)
                return BadRequest(new { message = "Valores numéricos não podem ser negativos." });

            // Confirma que a categoria informada realmente existe no banco antes de prosseguir
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
            if (category is null)
                return BadRequest(new { message = $"Categoria {product.CategoryId} não existe." });

            // Executa o UPDATE no banco e verifica se a operação afetou alguma linha
            var updated = await _repository.UpdateAsync(product);
            if (!updated) return NotFound(new { message = $"Produto {id} não encontrado." });

            return Ok(new { message = "Produto atualizado com sucesso.", product });
        }

        // Rota: PATCH /api/products/{id}
        // Atualiza parcialmente um produto — apenas os campos enviados no corpo serão alterados.
        // Útil quando o frontend quer mudar só o preço ou só a quantidade sem reenviar tudo.
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] ProductPatchRequest patch)
        {
            // Valida antecipadamente campos que chegaram para evitar chamadas desnecessárias ao banco
            if (patch.Name is not null && string.IsNullOrWhiteSpace(patch.Name))
                return BadRequest(new { message = "Nome não pode ser vazio." });

            if (patch.Quantity.HasValue && patch.Quantity.Value < 0)
                return BadRequest(new { message = "Quantidade não pode ser negativa." });

            if (patch.ReorderThreshold.HasValue && patch.ReorderThreshold.Value < 0)
                return BadRequest(new { message = "Limite de reposição não pode ser negativo." });

            if (patch.Price.HasValue && patch.Price.Value < 0)
                return BadRequest(new { message = "Preço não pode ser negativo." });

            // Se um novo CategoryId foi enviado, verifica se a categoria realmente existe
            if (patch.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(patch.CategoryId.Value);
                if (category is null)
                    return BadRequest(new { message = $"Categoria {patch.CategoryId.Value} não existe." });
            }

            try
            {
                var updated = await _repository.PatchAsync(id, patch);
                if (updated is null)
                    return NotFound(new { message = $"Produto {id} não encontrado." });

                return Ok(new { message = "Produto atualizado parcialmente com sucesso.", product = updated });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Rota: DELETE /api/products/{id}
        // Remove permanentemente um produto do banco de dados
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            // Se nenhuma linha foi afetada, o produto não existia
            if (!deleted) return NotFound(new { message = $"Produto {id} não encontrado." });
            return Ok(new { message = "Produto removido com sucesso." });
        }
    }
}