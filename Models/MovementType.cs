// Define o namespace agrupando as classes de modelo
namespace ApiEstoqueRoupas.Models
{
    // Um Enumerador (Enum) que define os únicos dois tipos de movimentações permitidas no estoque
    // Isso evita erros de digitação (ex: escrever "ENTRADDDA" no banco) e facilita as consultas
    public enum MovementType
    {
        // Representa adição de produtos ao estoque (ex: compras, devoluções)
        ENTRADA = 1,
        
        // Representa remoção de produtos do estoque (ex: vendas, perdas)
        SAIDA = 2
    }
}
