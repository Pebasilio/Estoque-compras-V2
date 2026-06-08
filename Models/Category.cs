// Importa o namespace necessário para usar coleções genéricas, como a classe List<T>
using System.Collections.Generic;

// Define o espaço de nomes (namespace) do projeto, organizando as classes relacionadas aos Modelos (entidades do banco)
namespace ApiEstoqueRoupas.Models
{
    // Declara a classe pública 'Category', que representa a entidade "Categoria" de produtos no sistema
    public class Category
    {
        // Propriedade pública de leitura e escrita que armazena o identificador único (Chave Primária) da categoria
        public int Id { get; set; }
        
        // Propriedade pública que armazena o nome da categoria. É inicializada com uma string vazia para evitar erros de valor nulo (NullReferenceException)
        public string Name { get; set; } = string.Empty;

        // Propriedade que cria uma lista de produtos. Isso representa o relacionamento "1 para N" (Uma categoria tem Vários produtos)
        public List<Product> Products { get; set; } = new List<Product>();

        // Construtor padrão sem parâmetros, necessário para ferramentas de banco de dados (como o Entity Framework/Dapper) criarem instâncias vazias
        public Category() { }

        // Construtor customizado que permite criar uma nova categoria já passando o nome dela como parâmetro
        public Category(string name)
        {
            // Atribui o valor do parâmetro 'name' (recebido no construtor) à propriedade 'Name' desta instância da classe
            Name = name;
        }
    }
}
