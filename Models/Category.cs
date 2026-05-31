using System.Collections.Generic;

namespace ApiEstoqueRoupas.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<Product> Products { get; set; } = new List<Product>();

        public Category() { }

        public Category(string name)
        {
            Name = name;
        }
    }
}
