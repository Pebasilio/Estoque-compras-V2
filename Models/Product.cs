using System;

// Namespace agrupa todos os modelos e DTOs (Data Transfer Objects) usados na aplicação
namespace ApiEstoqueRoupas.Models
{
    // ==========================================
    // ENTIDADE PRINCIPAL: PRODUTO
    // ==========================================
    public class Product
    {
        // Chave Primária no banco de dados
        public int Id { get; set; }
        
        // Nome da peça de roupa
        public string Name { get; set; } = string.Empty;
        
        // Quantidade atual disponível em estoque
        public int Quantity { get; set; }
        
        // Limite mínimo de estoque. Se a Quantity cair abaixo deste número, o sistema emitirá alertas de reposição
        public int ReorderThreshold { get; set; }
        
        // Valor unitário de venda da peça
        public decimal Price { get; set; }

        // Chave Estrangeira (Foreign Key) vinculando este produto a uma Categoria
        public int CategoryId { get; set; }
        // Propriedade de navegação: permite acessar os dados da Categoria a partir do Produto
        public Category? Category { get; set; }

        // Construtor vazio necessário para Entity Framework / Dapper
        public Product() { }

        // Construtor completo para facilitar a criação de produtos via código
        public Product(string name, int categoryId, int quantity, int reorderThreshold, decimal price)
        {
            Name = name;
            CategoryId = categoryId;
            Quantity = quantity;
            ReorderThreshold = reorderThreshold;
            Price = price;
        }

        // ==========================================
        // PROPRIEDADES CALCULADAS (LÓGICA DE NEGÓCIO)
        // ==========================================
        
        // Calcula dinamicamente o status de alerta do estoque no momento da leitura
        public string StockStatus
        {
            get
            {
                if (Quantity == 0) return "SEM_ESTOQUE"; // Crítico: Acabou totalmente
                if (Quantity <= ReorderThreshold) return "ESTOQUE_BAIXO"; // Alerta: Precisa repor
                return "OK"; // Saudável
            }
        }

        // Retorna o valor financeiro total deste produto armazenado no estoque (Preço x Quantidade)
        public decimal TotalStockValue => Quantity * Price;
    }

    // ==========================================
    // ENTIDADE: MOVIMENTAÇÃO DE ESTOQUE (HISTÓRICO)
    // ==========================================
    public class StockMovement
    {
        public int Id { get; set; }
        public int ProductId { get; set; } // Referência ao produto movimentado
        public string ProductName { get; set; } = string.Empty; // Nome salvo no momento da ação (histórico imutável)
        public MovementType Type { get; set; } // ENTRADA ou SAIDA
        public int Quantity { get; set; } // Quantidade movimentada nesta ação
        public int StockBefore { get; set; } // Retrato de como o estoque estava ANTES da ação
        public int StockAfter { get; set; } // Retrato de como o estoque ficou DEPOIS da ação
        public string Reason { get; set; } = string.Empty; // Motivo da movimentação (ex: "Venda" ou "Reposição")
        public DateTime Date { get; set; } = DateTime.Now; // Data e hora exatas da operação
        public string User { get; set; } = "Sistema"; // Operador que realizou a ação
        public Product? Product { get; set; } // Propriedade de navegação (opcional)
    }

    // ==========================================
    // DTOs (Data Transfer Objects)
    // Usados para receber requisições do Frontend de forma segura e controlada
    // ==========================================
    
    // Formato esperado quando o Front envia uma "Entrada" de estoque
    public class StockEntryRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = "Reposição";
        public string User { get; set; } = "Sistema";
    }

    // Formato esperado quando o Front envia uma "Saída" de estoque
    public class StockExitRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = "Venda";
        public string User { get; set; } = "Sistema";
    }

    // Resposta padrão enviada de volta ao Frontend após concluir qualquer movimentação
    public class StockMovementResponse
    {
        public bool Success { get; set; } // Indica se a operação funcionou
        public string Message { get; set; } = string.Empty; // Mensagem amigável para exibir na tela
        public StockMovement? Movement { get; set; } // Devolve o registro salvo
        public bool NeedsRestock { get; set; } // Flag ("Bandeira") rápida informando se a operação acionou o alarme de estoque baixo
        public int CurrentStock { get; set; } // Saldo final
        public int ReorderThreshold { get; set; } // Qual era o limite cadastrado
    }

    // DTO focado exclusivamente em representar a "Sugestão de Compra" na tela de Compras
    public class RestockAlert
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int ReorderThreshold { get; set; }
        public int SuggestedOrderQuantity { get; set; } // Cálculo inteligente: O quanto eu devo comprar para sair da zona de risco
        public string AlertLevel { get; set; } = string.Empty; // "WARNING" ou "CRITICAL"
    }
}
