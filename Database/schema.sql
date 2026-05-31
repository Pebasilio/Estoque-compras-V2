-- ============================================================
-- Script do banco de dados - Sistema de Controle de Estoque
-- SGBD: SQLite
-- Gerado a partir do modelo do Entity Framework Core
-- ============================================================

-- Tabela: Categories
CREATE TABLE IF NOT EXISTS "Categories" (
    "Id"   INTEGER NOT NULL CONSTRAINT "PK_Categories" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT    NOT NULL
);

-- Tabela: Products (FK -> Categories)
CREATE TABLE IF NOT EXISTS "Products" (
    "Id"               INTEGER        NOT NULL CONSTRAINT "PK_Products" PRIMARY KEY AUTOINCREMENT,
    "Name"             TEXT           NOT NULL,
    "Quantity"         INTEGER        NOT NULL,
    "ReorderThreshold" INTEGER        NOT NULL,
    "Price"            decimal(10,2)  NOT NULL,
    "CategoryId"       INTEGER        NOT NULL,
    CONSTRAINT "FK_Products_Categories_CategoryId"
        FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_Products_CategoryId" ON "Products" ("CategoryId");

-- Tabela: StockMovements (FK -> Products)
-- Type é persistido como string ("ENTRADA" | "SAIDA") via conversor do EF
CREATE TABLE IF NOT EXISTS "StockMovements" (
    "Id"           INTEGER  NOT NULL CONSTRAINT "PK_StockMovements" PRIMARY KEY AUTOINCREMENT,
    "ProductId"    INTEGER  NOT NULL,
    "ProductName"  TEXT     NOT NULL,
    "Type"         TEXT     NOT NULL,
    "Quantity"     INTEGER  NOT NULL,
    "StockBefore"  INTEGER  NOT NULL,
    "StockAfter"   INTEGER  NOT NULL,
    "Reason"       TEXT     NOT NULL,
    "Date"         TEXT     NOT NULL,
    "User"         TEXT     NOT NULL,
    CONSTRAINT "FK_StockMovements_Products_ProductId"
        FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_StockMovements_ProductId" ON "StockMovements" ("ProductId");

-- ============================================================
-- Seed inicial (categorias + produtos de exemplo)
-- ============================================================

INSERT INTO "Categories" ("Name") VALUES ('Camisas');
INSERT INTO "Categories" ("Name") VALUES ('Jaquetas');
INSERT INTO "Categories" ("Name") VALUES ('Calças');
INSERT INTO "Categories" ("Name") VALUES ('Meias');

-- Ids: 1 Camisas, 2 Jaquetas, 3 Calças, 4 Meias
INSERT INTO "Products" ("Name","CategoryId","Quantity","ReorderThreshold","Price") VALUES
('Camisa Polo Azul',        1, 30,  5,  89.90),
('Camisa Branca',            1, 25,  5,  59.90),
('Camisa Preta',             1, 40,  8,  59.90),
('Jaqueta Jeans',            2, 20,  3, 199.90),
('Jaqueta de Couro',         2, 10,  2, 499.90),
('Calça Jeans Azul',         3, 35,  6, 149.90),
('Calça Moletom Cinza',      3, 28,  4, 119.90),
('Calça Preta',              3, 18,  3, 129.90),
('Meias Brancas (par)',      4, 100, 20, 14.90),
('Meias Pretas (par)',       4, 80,  15, 14.90),
('Camisa Social Azul',       1, 25,  5, 129.90),
('Camisa Social Branca',     1, 30,  6, 129.90),
('Camisa Estampada',         1, 22,  4,  79.90),
('Jaqueta de Moletom',       2, 15,  3, 169.90),
('Jaqueta Puffer',           2, 12,  2, 289.90),
('Calça Cargo Verde',        3, 20,  5, 139.90),
('Calça Social Preta',       3, 17,  3, 159.90),
('Calça Jeans Clara',        3, 33,  6, 149.90),
('Meias Coloridas (par)',    4, 70,  10, 19.90),
('Meias Esportivas (par)',   4, 90,  15, 24.90);
