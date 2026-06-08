# Sistema de Controle de Estoque — Web API

Web API em **ASP.NET Core (C#)** para gerenciar um estoque de roupas. O sistema controla produtos, categorias e movimentações de estoque (entradas e saídas), com regras de negócio para alertas de reposição, cálculos de valor de inventário e relatórios.

## Participante
- Pedro Henrique Basilio
- Matheus Henrique Bueno
- Gabriel Ferraz Canoff

## Arquitetura

Projeto organizado em camadas:

```
/
├── Controllers/   # Endpoints da API (ProductsController, CategoriesController, StockController)
├── Models/        # Entidades (Product, Category, StockMovement) + enum MovementType + DTOs
├── Repositories/  # Repository Pattern (interfaces + implementações)
├── Data/          # DatabaseHelper (System.Data.SQLite)
├── Database/      # Script SQL (schema.sql) para criação manual do banco
├── Postman/       # Coleção Postman para teste dos endpoints
├── Program.cs     # Configuração, DI, seed do banco
└── estoque.db     # Banco SQLite (gerado automaticamente no 1º run)
```

Fluxo: **Controller → Repository → DatabaseHelper → SQLite**

## Tecnologias

- .NET 7 / C#
- ASP.NET Core Web API (Controllers)
- System.Data.SQLite (raw SQL)
- Repository Pattern
- Swagger (Swashbuckle)
- SQLite

## Entidades e Relacionamentos

- **Category** `1 ← N` **Product** (uma categoria possui vários produtos)
- **Product** `1 ← N` **StockMovement** (um produto possui várias movimentações)
- **Enum `MovementType`**: `ENTRADA`, `SAIDA`

## Regras de Negócio

- Validação de campos obrigatórios (Nome, Categoria válida, valores não-negativos).
- Saída de estoque bloqueia operação quando estoque é insuficiente.
- Alerta automático (`NeedsRestock`) quando `Quantity <= ReorderThreshold`.
- Classificação de alerta: `CRITICAL` (estoque zero) ou `WARNING`.
- Sugestão de reposição calculada: `(ReorderThreshold * 3) - Quantity`.
- Propriedade calculada `TotalStockValue` (Quantity × Price) e `StockStatus` (OK / ESTOQUE_BAIXO / SEM_ESTOQUE).
- Relatório agregado: total de produtos, unidades, valor de inventário, preço médio, entradas/saídas do dia.

## Endpoints

### Produtos — `/api/products`

| Método | Rota                           | Descrição                                            |
|--------|--------------------------------|------------------------------------------------------|
| GET    | `/api/products`                | Lista todos os produtos (incluindo a categoria)     |
| GET    | `/api/products/{id}`           | Busca um produto pelo ID                            |
| GET    | `/api/products/low-stock`      | Lista produtos com estoque abaixo do limite        |
| POST   | `/api/products`                | Cria um novo produto                                |
| PUT    | `/api/products/{id}`           | Atualiza um produto existente                       |
| DELETE | `/api/products/{id}`           | Remove um produto                                   |

### Categorias — `/api/categories`

| Método | Rota                     | Descrição                                     |
|--------|--------------------------|-----------------------------------------------|
| GET    | `/api/categories`        | Lista todas as categorias (com produtos)      |
| GET    | `/api/categories/{id}`   | Busca uma categoria pelo ID                   |
| POST   | `/api/categories`        | Cria nova categoria                           |
| PUT    | `/api/categories/{id}`   | Atualiza categoria                            |
| DELETE | `/api/categories/{id}`   | Remove categoria (apenas se sem produtos)     |

### Estoque / Movimentações — `/api/stock`

| Método | Rota                                 | Descrição                                                  |
|--------|--------------------------------------|------------------------------------------------------------|
| POST   | `/api/stock/entry`                   | Registra entrada de estoque                                |
| POST   | `/api/stock/exit`                    | Registra saída de estoque (valida disponibilidade)        |
| GET    | `/api/stock/history/{productId}`     | Histórico de movimentações do produto                     |
| GET    | `/api/stock/movements?type=ENTRADA`  | Movimentações (filtrável por `ENTRADA` ou `SAIDA`)        |
| GET    | `/api/stock/restock-alerts`          | Produtos que precisam de reposição (com sugestão)         |
| GET    | `/api/stock/report`                  | Relatório consolidado do estoque                          |

## Exemplos de Payload

### POST `/api/products`
```json
{
  "name": "Camiseta Regata",
  "categoryId": 1,
  "quantity": 20,
  "reorderThreshold": 5,
  "price": 39.90
}
```

### POST `/api/stock/entry`
```json
{
  "productId": 1,
  "quantity": 10,
  "reason": "Reposição de fornecedor",
  "user": "admin"
}
```

### POST `/api/stock/exit`
```json
{
  "productId": 1,
  "quantity": 2,
  "reason": "Venda balcão",
  "user": "vendedor"
}
```

## Como Executar

1. Instale o **.NET 7 SDK**.
2. No diretório do projeto, execute:

```bash
dotnet restore
dotnet run
```

3. Acesse:
   - API: `http://localhost:5123/api/products`
   - Swagger: `http://localhost:5123/swagger`

O banco `estoque.db` é criado automaticamente no primeiro start, com 4 categorias e 20 produtos de exemplo.

## Testes com Postman

A coleção está em `Postman/EstoqueAPI.postman_collection.json`. Importe no Postman (File → Import) — já vem com a variável `{{baseUrl}}` apontando para `http://localhost:5123` e requisições prontas para todos os endpoints (Products, Categories, Stock).

## Script do banco

O schema do banco é criado automaticamente pelo DatabaseHelper em `Data/DatabaseHelper.cs`, usando raw SQL. Um script equivalente está em `Database/schema.sql` para referência.

## Conceitos de C# aplicados

- **Classes e objetos** — `Product`, `Category`, `StockMovement`.
- **Propriedades** — incluindo calculadas (`StockStatus`, `TotalStockValue`).
- **Construtores** — `Category(string)`, `Product(string, int, int, int, decimal)`.
- **Collections** — `List<Product>`, `List<Category>`, `List<StockMovement>`.
- **Enum** — `MovementType { ENTRADA, SAIDA }`.
- **System.Data.SQLite** — Conexões diretas, comandos SQL, DataReader.
- **Repository Pattern** — `IProductRepository`, `ICategoryRepository`, `IStockMovementRepository`.
- **Injeção de dependência** — repositórios injetados nos controllers.

## Aplicação Front-end (React + Vite)

Este repositório agora inclui uma aplicação **Single Page Application (SPA)** desenvolvida com **React** e **Vite**, localizada dentro da pasta `frontend/`. O design utiliza as melhores práticas modernas (Glassmorphism e tipografia premium).

### Tecnologias do Front-end
- **React 18** e **Vite**
- **React Router DOM** (Gestão de rotas e rotas protegidas)
- **Lucide React** (Ícones modernos)
- **CSS Vanilla** (Variáveis dinâmicas para Design System)
- **Fetch API** na camada de serviços (`api.js`)

### Funcionalidades do Front-end
- **Autenticação:** Proteção de rotas baseada em JWT (com Mock Data disponível para testes sem backend).
- **Dashboard de KPIs:** Visualização analítica do total de produtos, valor em estoque, movimentações recentes e alertas críticos.
- **Gestão de Produtos (CRUD):** Interface intuitiva para visualização (grade e tabela), criação, edição e exclusão de itens (com dropdowns para categorias).
- **Controle de Estoque Rápido:** Modal flutuante para rápida entrada e saída de peças, registrando motivos e usuários (integra-se às rotas de `/api/stock`).
- **Simulador de Compras (Reposição):** Tela estratégica que lista apenas os produtos abaixo do mínimo estipulado, recomendando compras usando a fórmula `(Limite * 3) - Quantidade Atual`, com botão ágil que executa uma entrada de reposição automática.

### Como Executar o Sistema Completo (Backend + Frontend)

Para que a interface gráfica consiga ler o banco de dados SQLite, **é necessário rodar os dois servidores em terminais separados**.

1. **Terminal 1 - Iniciar a API C#:**
   Na pasta raiz do projeto (`Estoque-e-compras-main`), execute:
   ```bash
   dotnet run
   ```
   *(A API rodará na porta `5123` e começará a escutar os comandos)*

2. **Terminal 2 - Iniciar a Interface React:**
   Abra um segundo terminal, entre na pasta `frontend` e rode o projeto:
   ```bash
   cd frontend
   npm run dev
   ```
   *(O site abrirá em `http://localhost:5173` ou similar)*

A interface foi programada para capturar os retornos da API automaticamente. Se o C# não estiver rodando, você pode usar o modo "Mock Data" (dados de teste) alterando a constante `USE_MOCK_DATA` para `true` no arquivo `frontend/src/services/api.js`.
## Autenticacao JWT

O backend possui autenticacao JWT real usando `Microsoft.AspNetCore.Authentication.JwtBearer`.

### Endpoint de login

| Metodo | Rota              | Descricao                                 |
|--------|-------------------|-------------------------------------------|
| POST   | `/api/auth/login` | Valida credenciais e retorna um token JWT |

Payload:

```json
{
  "email": "admin@stockpro.com",
  "password": "admin123"
}
```

Resposta:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2026-06-08T12:00:00Z",
  "user": {
    "name": "Administrador",
    "email": "admin@stockpro.com",
    "role": "Admin"
  }
}
```

### Fluxo completo

1. O frontend envia e-mail e senha para `POST /api/auth/login`.
2. O backend valida as credenciais e gera um JWT assinado com a chave configurada em `appsettings.json`.
3. O frontend salva o token no `localStorage`.
4. As chamadas feitas por `fetchWithAuth` enviam o header `Authorization: Bearer <token>`.
5. Endpoints com `[Authorize]`, como `GET /api/stock/report`, respondem apenas quando o token e valido e nao expirou.

### Credenciais de demonstracao

| Perfil | E-mail | Senha |
|--------|--------|-------|
| Admin | `admin@stockpro.com` | `admin123` |
| Gestor | `gestor@stockpro.com` | `gestor123` |

### Endpoint protegido

`GET /api/stock/report` esta protegido com `[Authorize]`. Para testar no Postman ou no frontend, primeiro faca login, copie o token retornado e envie:

```http
Authorization: Bearer <token>
```
