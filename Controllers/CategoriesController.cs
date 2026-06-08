using System.Threading.Tasks;
using ApiEstoqueRoupas.Models;
using ApiEstoqueRoupas.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApiEstoqueRoupas.Controllers
{
    // ==========================================
    // CONTROLADOR DE CATEGORIAS
    // Expõe as rotas HTTP (Endpoints) que o React vai chamar via "fetch" ou "axios"
    // Caminho base: /api/categories
    // ==========================================
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        // Variável privada que guarda o repositório injetado
        private readonly ICategoryRepository _repository;

        // O ASP.NET Core injeta automaticamente o CategoryRepository aqui graças à configuração no Program.cs
        public CategoriesController(ICategoryRepository repository)
        {
            _repository = repository;
        }

        // Rota: GET /api/categories
        // Retorna a lista de todas as categorias
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _repository.GetAllAsync());

        // Rota: GET /api/categories/{id}
        // Busca uma categoria específica
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            // Retorna erro 404 se não achar, ou 200 OK com os dados
            return category is null ? NotFound(new { message = $"Categoria {id} não encontrada." }) : Ok(category);
        }

        // Rota: POST /api/categories
        // Cria uma nova categoria
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            // Validação de negócio básica: não permite criar categoria sem nome
            if (string.IsNullOrWhiteSpace(category.Name))
                return BadRequest(new { message = "Nome da categoria é obrigatório." });

            var created = await _repository.AddAsync(category);
            // Retorna 201 Created (Padrão REST) e informa o link para acessar a nova categoria gerada
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // Rota: PUT /api/categories/{id}
        // Atualiza os dados de uma categoria existente
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category category)
        {
            // Proteção contra inconsistência: o ID da URL deve bater com o ID do objeto enviado no corpo da requisição
            if (id != category.Id)
                return BadRequest(new { message = "ID da rota não corresponde ao ID do corpo." });
            if (string.IsNullOrWhiteSpace(category.Name))
                return BadRequest(new { message = "Nome da categoria é obrigatório." });

            // Tenta atualizar no banco. Se retornar false, a categoria não foi encontrada
            var updated = await _repository.UpdateAsync(category);
            if (!updated) return NotFound(new { message = $"Categoria {id} não encontrada." });
            return Ok(new { message = "Categoria atualizada.", category });
        }

        // Rota: DELETE /api/categories/{id}
        // Remove uma categoria. Falha se houver produtos vinculados (proteção de integridade referencial)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            // O repositório verifica internamente se existem produtos na categoria antes de deletar
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
                return BadRequest(new { message = "Categoria não encontrada ou possui produtos vinculados." });
            return Ok(new { message = "Categoria removida." });
        }
    }
}
