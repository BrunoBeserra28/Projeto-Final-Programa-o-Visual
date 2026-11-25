-- =====================================================================
-- SISTEMA DE GERENCIAMENTO DE PAPELARIA
-- Script completo: criação do banco, tabelas, views e massa de teste
-- SGBD: MySQL 8.0+
-- =====================================================================

DROP DATABASE IF EXISTS papelaria;
CREATE DATABASE papelaria
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;
USE papelaria;

-- ---------------------------------------------------------------------
-- 1. USUARIOS (login do sistema)
-- senha_hash = SHA-256 (hexadecimal minúsculo) da senha
-- ---------------------------------------------------------------------
CREATE TABLE usuarios (
    id_usuario   INT AUTO_INCREMENT PRIMARY KEY,
    nome         VARCHAR(100) NOT NULL,
    login        VARCHAR(50)  NOT NULL UNIQUE,
    senha_hash   CHAR(64)     NOT NULL,
    perfil       ENUM('ADMIN','VENDEDOR','ESTOQUISTA') NOT NULL DEFAULT 'VENDEDOR',
    ativo        TINYINT(1)   NOT NULL DEFAULT 1,
    criado_em    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 2. CLIENTES
-- ---------------------------------------------------------------------
CREATE TABLE clientes (
    id_cliente   INT AUTO_INCREMENT PRIMARY KEY,
    nome         VARCHAR(100) NOT NULL,
    cpf          VARCHAR(14)  NOT NULL UNIQUE,
    telefone     VARCHAR(20),
    email        VARCHAR(100),
    endereco     VARCHAR(200),
    cidade       VARCHAR(60),
    uf           CHAR(2),
    criado_em    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 3. CATEGORIAS
-- ---------------------------------------------------------------------
CREATE TABLE categorias (
    id_categoria INT AUTO_INCREMENT PRIMARY KEY,
    nome         VARCHAR(60)  NOT NULL UNIQUE,
    descricao    VARCHAR(200)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 4. FORNECEDORES
-- ---------------------------------------------------------------------
CREATE TABLE fornecedores (
    id_fornecedor INT AUTO_INCREMENT PRIMARY KEY,
    razao_social  VARCHAR(120) NOT NULL,
    cnpj          VARCHAR(18)  NOT NULL UNIQUE,
    telefone      VARCHAR(20),
    email         VARCHAR(100),
    endereco      VARCHAR(200),
    ativo         TINYINT(1)   NOT NULL DEFAULT 1
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 5. PRODUTOS (N:1 com categorias)
-- ---------------------------------------------------------------------
CREATE TABLE produtos (
    id_produto    INT AUTO_INCREMENT PRIMARY KEY,
    codigo_barras VARCHAR(30)    UNIQUE,
    nome          VARCHAR(120)   NOT NULL,
    descricao     VARCHAR(250),
    preco_custo   DECIMAL(10,2)  NOT NULL DEFAULT 0 CHECK (preco_custo >= 0),
    preco_venda   DECIMAL(10,2)  NOT NULL CHECK (preco_venda >= 0),
    id_categoria  INT            NOT NULL,
    ativo         TINYINT(1)     NOT NULL DEFAULT 1,
    CONSTRAINT fk_produto_categoria FOREIGN KEY (id_categoria)
        REFERENCES categorias(id_categoria)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 6. ESTOQUE (1:1 com produtos)
-- ---------------------------------------------------------------------
CREATE TABLE estoque (
    id_produto     INT PRIMARY KEY,
    quantidade     INT NOT NULL DEFAULT 0 CHECK (quantidade >= 0),
    estoque_minimo INT NOT NULL DEFAULT 5 CHECK (estoque_minimo >= 0),
    atualizado_em  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_estoque_produto FOREIGN KEY (id_produto)
        REFERENCES produtos(id_produto) ON DELETE CASCADE
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 7. VENDAS (N:1 com clientes e usuarios)
-- ---------------------------------------------------------------------
CREATE TABLE vendas (
    id_venda      INT AUTO_INCREMENT PRIMARY KEY,
    data_venda    DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    id_cliente    INT           NULL,            -- NULL = venda sem cliente identificado
    id_usuario    INT           NOT NULL,
    forma_pagto   ENUM('DINHEIRO','CARTAO_DEBITO','CARTAO_CREDITO','PIX') NOT NULL DEFAULT 'DINHEIRO',
    valor_total   DECIMAL(12,2) NOT NULL DEFAULT 0 CHECK (valor_total >= 0),
    CONSTRAINT fk_venda_cliente FOREIGN KEY (id_cliente) REFERENCES clientes(id_cliente),
    CONSTRAINT fk_venda_usuario FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 8. ITENS_VENDA  (N:N  Venda <-> Produto)
-- ---------------------------------------------------------------------
CREATE TABLE itens_venda (
    id_venda       INT NOT NULL,
    id_produto     INT NOT NULL,
    quantidade     INT NOT NULL CHECK (quantidade > 0),
    preco_unitario DECIMAL(10,2) NOT NULL CHECK (preco_unitario >= 0),
    desconto       DECIMAL(10,2) NOT NULL DEFAULT 0 CHECK (desconto >= 0),
    PRIMARY KEY (id_venda, id_produto),
    CONSTRAINT fk_iv_venda   FOREIGN KEY (id_venda)   REFERENCES vendas(id_venda) ON DELETE CASCADE,
    CONSTRAINT fk_iv_produto FOREIGN KEY (id_produto) REFERENCES produtos(id_produto)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 9. PEDIDOS_COMPRA (N:1 com fornecedores e usuarios)
-- ---------------------------------------------------------------------
CREATE TABLE pedidos_compra (
    id_pedido     INT AUTO_INCREMENT PRIMARY KEY,
    data_pedido   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    id_fornecedor INT      NOT NULL,
    id_usuario    INT      NOT NULL,
    status        ENUM('ABERTO','RECEBIDO','CANCELADO') NOT NULL DEFAULT 'ABERTO',
    valor_total   DECIMAL(12,2) NOT NULL DEFAULT 0,
    CONSTRAINT fk_pc_fornecedor FOREIGN KEY (id_fornecedor) REFERENCES fornecedores(id_fornecedor),
    CONSTRAINT fk_pc_usuario    FOREIGN KEY (id_usuario)    REFERENCES usuarios(id_usuario)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 10. ITENS_PEDIDO_COMPRA  (N:N  PedidoCompra <-> Produto)
-- ---------------------------------------------------------------------
CREATE TABLE itens_pedido_compra (
    id_pedido      INT NOT NULL,
    id_produto     INT NOT NULL,
    quantidade     INT NOT NULL CHECK (quantidade > 0),
    custo_unitario DECIMAL(10,2) NOT NULL CHECK (custo_unitario >= 0),
    PRIMARY KEY (id_pedido, id_produto),
    CONSTRAINT fk_ipc_pedido  FOREIGN KEY (id_pedido)  REFERENCES pedidos_compra(id_pedido) ON DELETE CASCADE,
    CONSTRAINT fk_ipc_produto FOREIGN KEY (id_produto) REFERENCES produtos(id_produto)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 11. PRODUTO_FORNECEDOR  (N:N  Fornecedor <-> Produto)
-- ---------------------------------------------------------------------
CREATE TABLE produto_fornecedor (
    id_produto      INT NOT NULL,
    id_fornecedor   INT NOT NULL,
    custo_fornecedor DECIMAL(10,2) NOT NULL DEFAULT 0,
    prazo_entrega_dias INT NOT NULL DEFAULT 7,
    PRIMARY KEY (id_produto, id_fornecedor),
    CONSTRAINT fk_pf_produto    FOREIGN KEY (id_produto)    REFERENCES produtos(id_produto) ON DELETE CASCADE,
    CONSTRAINT fk_pf_fornecedor FOREIGN KEY (id_fornecedor) REFERENCES fornecedores(id_fornecedor) ON DELETE CASCADE
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 12. PROMOCOES
-- ---------------------------------------------------------------------
CREATE TABLE promocoes (
    id_promocao   INT AUTO_INCREMENT PRIMARY KEY,
    nome          VARCHAR(100) NOT NULL,
    percentual    DECIMAL(5,2) NOT NULL CHECK (percentual > 0 AND percentual <= 100),
    data_inicio   DATE NOT NULL,
    data_fim      DATE NOT NULL,
    ativa         TINYINT(1) NOT NULL DEFAULT 1,
    CHECK (data_fim >= data_inicio)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 13. PROMOCAO_PRODUTO  (N:N  Promoção <-> Produto)
-- ---------------------------------------------------------------------
CREATE TABLE promocao_produto (
    id_promocao INT NOT NULL,
    id_produto  INT NOT NULL,
    PRIMARY KEY (id_promocao, id_produto),
    CONSTRAINT fk_pp_promocao FOREIGN KEY (id_promocao) REFERENCES promocoes(id_promocao) ON DELETE CASCADE,
    CONSTRAINT fk_pp_produto  FOREIGN KEY (id_produto)  REFERENCES produtos(id_produto) ON DELETE CASCADE
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 14. MOVIMENTACOES_ESTOQUE (histórico de entradas e saídas)
-- ---------------------------------------------------------------------
CREATE TABLE movimentacoes_estoque (
    id_movimentacao INT AUTO_INCREMENT PRIMARY KEY,
    id_produto      INT NOT NULL,
    tipo            ENUM('ENTRADA','SAIDA') NOT NULL,
    quantidade      INT NOT NULL CHECK (quantidade > 0),
    data_mov        DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    motivo          VARCHAR(100) NOT NULL,
    id_usuario      INT NOT NULL,
    id_venda        INT NULL,       -- preenchido quando a saída vem de uma venda
    id_pedido       INT NULL,       -- preenchido quando a entrada vem de um pedido de compra
    CONSTRAINT fk_me_produto FOREIGN KEY (id_produto) REFERENCES produtos(id_produto),
    CONSTRAINT fk_me_usuario FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    CONSTRAINT fk_me_venda   FOREIGN KEY (id_venda)   REFERENCES vendas(id_venda),
    CONSTRAINT fk_me_pedido  FOREIGN KEY (id_pedido)  REFERENCES pedidos_compra(id_pedido)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 15. AUDITORIA (diferencial: registro de operações)
-- ---------------------------------------------------------------------
CREATE TABLE auditoria (
    id_auditoria INT AUTO_INCREMENT PRIMARY KEY,
    data_hora    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    id_usuario   INT NULL,
    acao         VARCHAR(50)  NOT NULL,
    detalhe      VARCHAR(255),
    CONSTRAINT fk_aud_usuario FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- ÍNDICES AUXILIARES (desempenho de pesquisas e relatórios)
-- ---------------------------------------------------------------------
CREATE INDEX idx_produtos_nome     ON produtos(nome);
CREATE INDEX idx_produtos_categoria ON produtos(id_categoria);
CREATE INDEX idx_clientes_nome     ON clientes(nome);
CREATE INDEX idx_vendas_data       ON vendas(data_venda);
CREATE INDEX idx_mov_produto       ON movimentacoes_estoque(id_produto, data_mov);

-- ---------------------------------------------------------------------
-- VIEWS
-- ---------------------------------------------------------------------
CREATE VIEW vw_estoque_baixo AS
SELECT p.id_produto, p.nome, c.nome AS categoria, e.quantidade, e.estoque_minimo
FROM produtos p
JOIN estoque e    ON e.id_produto = p.id_produto
JOIN categorias c ON c.id_categoria = p.id_categoria
WHERE p.ativo = 1 AND e.quantidade <= e.estoque_minimo;

CREATE VIEW vw_relatorio_vendas AS
SELECT v.id_venda, v.data_venda, COALESCE(cl.nome,'Consumidor final') AS cliente,
       u.nome AS vendedor, v.forma_pagto, v.valor_total
FROM vendas v
LEFT JOIN clientes cl ON cl.id_cliente = v.id_cliente
JOIN usuarios u       ON u.id_usuario  = v.id_usuario;

-- =====================================================================
-- MASSA DE TESTE
-- =====================================================================

-- Senhas: admin123 / vendas123 / estoque123  (SHA-256)
INSERT INTO usuarios (nome, login, senha_hash, perfil) VALUES
('Administrador', 'admin',    SHA2('admin123', 256),   'ADMIN'),
('Maria Vendedora','maria',   SHA2('vendas123', 256),  'VENDEDOR'),
('João Estoquista','joao',    SHA2('estoque123', 256), 'ESTOQUISTA');

INSERT INTO categorias (nome, descricao) VALUES
('Material Escolar', 'Lápis, borrachas, réguas, mochilas'),
('Material de Escritório', 'Grampeadores, pastas, clipes'),
('Informática', 'Pen drives, mouses, teclados'),
('Livros', 'Livros didáticos e literatura'),
('Cadernos', 'Cadernos e blocos'),
('Canetas', 'Canetas, marca-textos e lapiseiras'),
('Impressoras', 'Impressoras e multifuncionais'),
('Cartuchos e Toners', 'Suprimentos para impressão');

INSERT INTO fornecedores (razao_social, cnpj, telefone, email, endereco) VALUES
('Papel & Cia Distribuidora Ltda', '12.345.678/0001-90', '(11) 3333-1111', 'vendas@papelecia.com.br', 'Rua das Flores, 100 - São Paulo/SP'),
('TechSupri Informática S/A',      '98.765.432/0001-10', '(11) 4444-2222', 'contato@techsupri.com.br', 'Av. Paulista, 2000 - São Paulo/SP'),
('Editora Saber Ltda',             '11.222.333/0001-44', '(11) 5555-3333', 'comercial@editorasaber.com.br', 'Rua do Livro, 55 - Campinas/SP'),
('ImpressaoMax Suprimentos',       '55.666.777/0001-88', '(19) 2222-4444', 'pedidos@impressaomax.com.br', 'Av. Brasil, 800 - Campinas/SP');

INSERT INTO clientes (nome, cpf, telefone, email, endereco, cidade, uf) VALUES
('Ana Souza',      '111.111.111-11', '(11) 98888-1111', 'ana@email.com',    'Rua A, 10', 'São Paulo', 'SP'),
('Bruno Lima',     '222.222.222-22', '(11) 97777-2222', 'bruno@email.com',  'Rua B, 20', 'São Paulo', 'SP'),
('Carla Mendes',   '333.333.333-33', '(11) 96666-3333', 'carla@email.com',  'Rua C, 30', 'Osasco',    'SP'),
('Diego Ferreira', '444.444.444-44', '(11) 95555-4444', 'diego@email.com',  'Rua D, 40', 'Guarulhos', 'SP'),
('Elisa Rocha',    '555.555.555-55', '(11) 94444-5555', 'elisa@email.com',  'Rua E, 50', 'Santo André','SP');

INSERT INTO produtos (codigo_barras, nome, descricao, preco_custo, preco_venda, id_categoria) VALUES
('7890000000011', 'Lápis HB nº2',              'Caixa unitária',          0.80,   1.50, 1),
('7890000000012', 'Borracha Branca',           'Borracha macia',          0.60,   1.20, 1),
('7890000000013', 'Mochila Escolar 20L',       'Mochila resistente',     55.00, 109.90, 1),
('7890000000021', 'Grampeador de Mesa',        'Grampeia até 25 folhas', 14.00,  29.90, 2),
('7890000000022', 'Pasta Suspensa (10 un)',    'Pacote com 10',          12.00,  24.90, 2),
('7890000000031', 'Pen Drive 64GB',            'USB 3.0',                22.00,  44.90, 3),
('7890000000032', 'Mouse Óptico USB',          'Mouse com fio',          12.00,  25.90, 3),
('7890000000033', 'Teclado ABNT2 USB',         'Teclado com fio',        28.00,  59.90, 3),
('7890000000041', 'Dom Casmurro',              'Machado de Assis',       15.00,  34.90, 4),
('7890000000042', 'Matemática Básica Vol. 1',  'Livro didático',         40.00,  89.90, 4),
('7890000000051', 'Caderno Universitário 10M', '200 folhas',             12.00,  24.90, 5),
('7890000000052', 'Caderno Brochura 96 fls',   'Capa dura',               6.00,  13.90, 5),
('7890000000061', 'Caneta Esferográfica Azul', 'Ponta 1.0mm',             0.90,   2.50, 6),
('7890000000062', 'Marca-texto Amarelo',       'Ponta chanfrada',         1.80,   4.50, 6),
('7890000000063', 'Lapiseira 0.7mm',           'Corpo metálico',          5.00,  12.90, 6),
('7890000000071', 'Impressora Jato de Tinta',  'Multifuncional Wi-Fi',  320.00, 599.00, 7),
('7890000000081', 'Cartucho Preto 664',        'Compatível HP',          35.00,  69.90, 8),
('7890000000082', 'Cartucho Colorido 664',     'Compatível HP',          42.00,  79.90, 8),
('7890000000083', 'Toner Laser Universal',     'Alto rendimento',        90.00, 169.90, 8);

-- Estoque inicial (alguns itens abaixo do mínimo para demonstrar o alerta)
INSERT INTO estoque (id_produto, quantidade, estoque_minimo) VALUES
(1,200,50),(2,150,40),(3,15,5),(4,20,5),(5,30,10),(6,25,8),(7,30,10),(8,12,5),
(9,18,5),(10,10,5),(11,60,20),(12,80,20),(13,500,100),(14,90,30),(15,40,10),
(16,4,3),(17,3,5),(18,2,5),(19,6,3);

INSERT INTO produto_fornecedor (id_produto, id_fornecedor, custo_fornecedor, prazo_entrega_dias) VALUES
(1,1,0.80,5),(2,1,0.60,5),(3,1,55.00,7),(4,1,14.00,5),(5,1,12.00,5),
(11,1,12.00,5),(12,1,6.00,5),(13,1,0.90,3),(14,1,1.80,3),(15,1,5.00,4),
(6,2,22.00,4),(7,2,12.00,4),(8,2,28.00,4),(16,2,320.00,10),
(9,3,15.00,10),(10,3,40.00,10),
(17,4,35.00,6),(18,4,42.00,6),(19,4,90.00,6),(16,4,325.00,8),(6,4,23.00,6);

INSERT INTO promocoes (nome, percentual, data_inicio, data_fim) VALUES
('Volta às Aulas',     10.00, '2026-01-15', '2026-02-28'),
('Semana da Informática', 15.00, '2026-11-20', '2026-12-31'),
('Liquidação de Cartuchos', 20.00, '2026-11-01', '2026-12-15');

INSERT INTO promocao_produto (id_promocao, id_produto) VALUES
(1,1),(1,2),(1,3),(1,11),(1,12),(1,13),
(2,6),(2,7),(2,8),
(3,17),(3,18);

-- Vendas de exemplo para o histórico/relatório (valor_total = soma(qtd*preço - desconto))
INSERT INTO vendas (data_venda, id_cliente, id_usuario, forma_pagto, valor_total) VALUES
('2026-11-10 10:15:00', 1,    2, 'PIX',            29.50),
('2026-11-12 14:30:00', 2,    2, 'CARTAO_CREDITO', 130.20),
('2026-11-15 09:05:00', NULL, 2, 'DINHEIRO',       22.60),
('2026-11-18 16:45:00', 3,    1, 'CARTAO_DEBITO',  599.00),
('2026-11-20 11:20:00', 4,    2, 'PIX',            150.60);

INSERT INTO itens_venda (id_venda, id_produto, quantidade, preco_unitario, desconto) VALUES
(1, 11, 1, 24.90, 0), (1, 14, 1, 4.50, 0), (1, 1, 1, 1.50, 1.40),
(2, 6, 2, 44.90, 0),  (2, 15, 1, 12.90, 0), (2, 2, 3, 1.20, 0), (2, 13, 4, 2.50, 0), (2, 12, 1, 13.90, 0),
(3, 13, 3, 2.50, 0),  (3, 12, 1, 13.90, 0), (3, 2, 1, 1.20, 0),
(4, 16, 1, 599.00, 0),
(5, 8, 1, 59.90, 0),  (5, 7, 1, 25.90, 0),  (5, 4, 1, 29.90, 0),  (5, 9, 1, 34.90, 0);

INSERT INTO movimentacoes_estoque (id_produto, tipo, quantidade, data_mov, motivo, id_usuario, id_venda) VALUES
(11,'SAIDA',1,'2026-11-10 10:15:00','Venda #1',2,1),
(14,'SAIDA',1,'2026-11-10 10:15:00','Venda #1',2,1),
(1,'SAIDA',1,'2026-11-10 10:15:00','Venda #1',2,1),
(16,'SAIDA',1,'2026-11-18 16:45:00','Venda #4',1,4);

INSERT INTO pedidos_compra (data_pedido, id_fornecedor, id_usuario, status, valor_total) VALUES
('2026-11-05 09:00:00', 4, 1, 'RECEBIDO', 1050.00),
('2026-11-22 10:00:00', 2, 1, 'ABERTO',   3200.00);

INSERT INTO itens_pedido_compra (id_pedido, id_produto, quantidade, custo_unitario) VALUES
(1, 17, 10, 35.00), (1, 18, 5, 42.00), (1, 19, 5, 90.00),
(2, 16, 10, 320.00);

INSERT INTO movimentacoes_estoque (id_produto, tipo, quantidade, data_mov, motivo, id_usuario, id_pedido) VALUES
(17,'ENTRADA',10,'2026-11-06 08:30:00','Recebimento do pedido #1',1,1),
(18,'ENTRADA',5, '2026-11-06 08:30:00','Recebimento do pedido #1',1,1),
(19,'ENTRADA',5, '2026-11-06 08:30:00','Recebimento do pedido #1',1,1);

INSERT INTO auditoria (id_usuario, acao, detalhe) VALUES
(1, 'CARGA_INICIAL', 'Massa de teste carregada pelo script SQL');

-- Conferência rápida
-- SELECT * FROM vw_estoque_baixo;
-- SELECT * FROM vw_relatorio_vendas;
