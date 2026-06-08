using ApiEstoqueRoupas.Data;
using ApiEstoqueRoupas.Models;
using ApiEstoqueRoupas.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Data.SQLite;
using System.Text;

// ==========================================
// ARQUIVO PRINCIPAL DO BACKEND (Ponto de Entrada)
// ==========================================

// Inicia o construtor da aplicação web usando o padrão mínimo do ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

// Define onde o banco de dados SQLite será criado (na raiz do projeto como 'estoque.db')
var connectionString = "Data Source=estoque.db";

// ==========================================
// INJEÇÃO DE DEPENDÊNCIA (Dependency Injection)
// Ensina ao C# como criar as classes quando os Controllers pedirem.
// ==========================================
// AddSingleton: Cria uma única instância (Global) que dura a vida toda da API. Perfeito para a string de conexão.
builder.Services.AddSingleton(new DatabaseHelper(connectionString));

// AddScoped: Cria uma nova instância por cada requisição HTTP recebida. Ideal para Repositórios.
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();

// ==========================================
// CONFIGURAÇÕES DA API (Controllers, JSON e CORS)
// ==========================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Evita loops infinitos ao serializar objetos (ex: Produto tem Categoria que tem Produto que tem Categoria...)
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        
        // Transforma Enum's em Texto (ex: "ENTRADA") ao invés de números (1 ou 2) na hora de devolver no JSON
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Ativa o Swagger (Página bonita de documentação /swagger para testar a API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==========================================
// CONFIGURAÇÃO DE AUTENTICAÇÃO JWT (JSON Web Token)
// O JWT é o "crachá digital" que o usuário recebe ao fazer login.
// A cada requisição, ele envia esse token para provar que já se autenticou.
// ==========================================

// Lê a chave secreta usada para assinar os tokens (deve estar em appsettings.json)
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada.");
// Emissor (quem gerou o token) e audiência (para quem o token serve)
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Registra o esquema de autenticação via Bearer Token (padrão JWT)
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Define as regras de validação do token recebido em cada requisição
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,           // Verifica se o emissor do token é confiável
            ValidateAudience = true,         // Verifica se o token foi destinado a esta API
            ValidateLifetime = true,         // Rejeita tokens expirados automaticamente
            ValidateIssuerSigningKey = true,  // Valida que a assinatura do token é legítima
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            // Converte a chave secreta de string para bytes para poder assinar/verificar criptograficamente
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero        // Remove a margem de tolerância padrão de 5 minutos na expiração
        };
    });

// Habilita o serviço de autorização (controle de permissões por Role: Admin, Manager, etc.)
builder.Services.AddAuthorization();

// Permite que sites em outras portas (como o React na 5173) consigam chamar a nossa API na porta 5123
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// "Constrói" o aplicativo e finaliza as configurações
var app = builder.Build();

// ==========================================
// INICIALIZAÇÃO DE DADOS (Seeding)
// ==========================================

// Solicita a classe DatabaseHelper recém criada e executa o CREATE TABLE das tabelas
var databaseHelper = app.Services.GetRequiredService<DatabaseHelper>();
databaseHelper.Initialize();

// Rotina para popular dados iniciais automaticamente se o banco estiver 100% vazio
using (var connection = new SQLiteConnection(connectionString))
{
    connection.Open();
    using (var command = connection.CreateCommand())
    {
        // Verifica se a tabela de categorias está vazia para decidir se deve popular dados iniciais
        command.CommandText = "SELECT COUNT(*) FROM Categories";
        var count = (long)command.ExecuteScalar();

        if (count == 0)
        {
            // Insere as categorias iniciais no banco e armazena seus IDs gerados
            var categories = new[] { "Camisas", "Jaquetas", "Calças", "Meias" };
            // Dicionário para mapear o nome da categoria ao seu ID gerado, usado depois ao inserir produtos
            var categoryIds = new Dictionary<string, int>();

            foreach (var categoryName in categories)
            {
                using (var cmd = connection.CreateCommand())
                {
                    // Insere a categoria e recupera o ID auto-incrementado pelo SQLite
                    cmd.CommandText = "INSERT INTO Categories (Name) VALUES (@Name); SELECT last_insert_rowid();";
                    cmd.Parameters.AddWithValue("@Name", categoryName);
                    var id = (long)cmd.ExecuteScalar();
                    categoryIds[categoryName] = (int)id; // Guarda para vincular aos produtos depois
                }
            }

            // Dados pré-definidos (Seed Data): lista de produtos fictícios para demonstração
            // Formato: (nome, categoria, quantidade, limiar de reposição, preço unitário)
            var produtosIniciais = new List<(string name, string category, int quantity, int reorderThreshold, decimal price)>
            {
                ("Camisa Polo Azul", "Camisas", 30, 5, 89.90m),
                ("Camisa Branca", "Camisas", 25, 5, 59.90m),
                ("Camisa Preta", "Camisas", 40, 8, 59.90m),
                ("Jaqueta Jeans", "Jaquetas", 20, 3, 199.90m),
                ("Jaqueta de Couro", "Jaquetas", 10, 2, 499.90m),
                ("Calça Jeans Azul", "Calças", 35, 6, 149.90m),
                ("Calça Moletom Cinza", "Calças", 28, 4, 119.90m),
                ("Calça Preta", "Calças", 18, 3, 129.90m),
                ("Meias Brancas (par)", "Meias", 100, 20, 14.90m),
                ("Meias Pretas (par)", "Meias", 80, 15, 14.90m),
                ("Camisa Social Azul", "Camisas", 25, 5, 129.90m),
                ("Camisa Social Branca", "Camisas", 30, 6, 129.90m),
                ("Camisa Estampada", "Camisas", 22, 4, 79.90m),
                ("Jaqueta de Moletom", "Jaquetas", 15, 3, 169.90m),
                ("Jaqueta Puffer", "Jaquetas", 12, 2, 289.90m),
                ("Calça Cargo Verde", "Calças", 20, 5, 139.90m),
                ("Calça Social Preta", "Calças", 17, 3, 159.90m),
                ("Calça Jeans Clara", "Calças", 33, 6, 149.90m),
                ("Meias Coloridas (par)", "Meias", 70, 10, 19.90m),
                ("Meias Esportivas (par)", "Meias", 90, 15, 24.90m)
            };

            // Percorre cada produto da lista e insere no banco vinculando ao ID da categoria correta
            foreach (var (name, category, quantity, reorderThreshold, price) in produtosIniciais)
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO Products (Name, Quantity, ReorderThreshold, Price, CategoryId)
                        VALUES (@Name, @Quantity, @ReorderThreshold, @Price, @CategoryId)";

                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@ReorderThreshold", reorderThreshold);
                    cmd.Parameters.AddWithValue("@Price", price);
                    // Busca o ID numérico da categoria pelo nome usando o dicionário preenchido anteriormente
                    cmd.Parameters.AddWithValue("@CategoryId", categoryIds[category]);

                    cmd.ExecuteNonQuery();
                }
            }

            Console.WriteLine($"Banco criado com {produtosIniciais.Count} produtos e 4 categorias.");
        }
        else
        {
            // Se o banco já possui dados, apenas exibe a contagem para conferência no terminal
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Products";
                var productCount = (long)cmd.ExecuteScalar();
                Console.WriteLine($"Banco existente com {productCount} produtos.");
            }
        }
    }
}

// ==========================================
// MIDDLEWARES E EXECUÇÃO
// ==========================================

app.UseSwagger();
app.UseSwaggerUI(); // Gera a interface gráfica do Swagger no navegador

app.UseCors("AllowAll"); // Libera o acesso CORS configurado lá em cima

app.UseAuthentication(); // Ativa a verificação do token JWT em cada requisição
app.UseAuthorization();  // Ativa a verificação de permissões (Roles) nos endpoints protegidos com [Authorize]

app.MapControllers(); // Mapeia todos as rotas definidas nos arquivos Controllers/

// Mensagens no terminal para ajudar o desenvolvedor a encontrar as URLs
Console.WriteLine("\nSERVIDOR RODANDO");
Console.WriteLine("  Swagger:    http://localhost:5123/swagger");
Console.WriteLine("  Produtos:   http://localhost:5123/api/products");
Console.WriteLine("  Categorias: http://localhost:5123/api/categories");
Console.WriteLine("  Estoque:    http://localhost:5123/api/stock/...\n");

app.Run(); // Coloca a API no ar para começar a receber requisições!
