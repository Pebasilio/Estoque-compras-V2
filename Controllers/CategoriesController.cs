using System.Threading.Tasks;
using ApiEstoqueRoupas.Models;
using ApiEstoqueRoupas.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApiEstoqueRoupas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _repository;

        public CategoriesController(ICategoryRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _repository.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            return category is null ? NotFound(new { message = $"Categoria {id} não encontrada." }) : Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                return BadRequest(new { message = "Nome da categoria é obrigatório." });

            var created = await _repository.AddAsync(category);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category category)
        {
            if (id != category.Id)
                return BadRequest(new { message = "ID da rota não corresponde ao ID do corpo." });
            if (string.IsNullOrWhiteSpace(category.Name))
                return BadRequest(new { message = "Nome da categoria é obrigatório." });

            var updated = await _repository.UpdateAsync(category);
            if (!updated) return NotFound(new { message = $"Categoria {id} não encontrada." });
            return Ok(new { message = "Categoria atualizada.", category });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
                return BadRequest(new { message = "Categoria não encontrada ou possui produtos vinculados." });
            return Ok(new { message = "Categoria removida." });
        }
    }
}
