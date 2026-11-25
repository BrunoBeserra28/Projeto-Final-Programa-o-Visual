using System.Data;
using MySqlConnector;
using PapelariaApp.Data;
using PapelariaApp.Model;

namespace PapelariaApp.DAO
{
    public class ProdutoDAO
    {
        /// <summary>Pesquisa por nome/código de barras com filtro opcional de categoria (0 = todas).</summary>
        public DataTable Listar(string filtro, int idCategoria) => Db.Consultar(
            @"SELECT p.id_produto AS Id, p.codigo_barras AS Codigo, p.nome AS Nome, c.nome AS Categoria,
                     p.preco_custo AS Custo, p.preco_venda AS Preco, COALESCE(e.quantidade,0) AS Estoque,
                     p.id_categoria AS IdCategoria, p.descricao AS Descricao, COALESCE(e.estoque_minimo,0) AS Minimo
              FROM produtos p
              JOIN categorias c ON c.id_categoria = p.id_categoria
              LEFT JOIN estoque e ON e.id_produto = p.id_produto
              WHERE p.ativo = 1
                AND (p.nome LIKE @f OR p.codigo_barras LIKE @f)
                AND (@cat = 0 OR p.id_categoria = @cat)
              ORDER BY p.nome",
            Db.P("@f", "%" + filtro + "%"), Db.P("@cat", idCategoria));

        public DataTable ListarCombo() =>
            Db.Consultar("SELECT id_produto AS Id, nome AS Nome FROM produtos WHERE ativo=1 ORDER BY nome");

        /// <summary>Produtos disponíveis para venda (com saldo), já com o saldo no texto.</summary>
        public DataTable ListarParaVenda(string filtro) => Db.Consultar(
            @"SELECT p.id_produto AS Id,
                     CONCAT(p.nome, '  -  R$ ', FORMAT(p.preco_venda,2), '  (estoque: ', e.quantidade, ')') AS Texto
              FROM produtos p JOIN estoque e ON e.id_produto = p.id_produto
              WHERE p.ativo=1 AND e.quantidade > 0 AND (p.nome LIKE @f OR p.codigo_barras LIKE @f)
              ORDER BY p.nome",
            Db.P("@f", "%" + filtro + "%"));

        public Produto ObterPorId(int id)
        {
            var dt = Db.Consultar(
                @"SELECT p.*, COALESCE(e.quantidade,0) AS qtd, COALESCE(e.estoque_minimo,0) AS minimo
                  FROM produtos p LEFT JOIN estoque e ON e.id_produto = p.id_produto
                  WHERE p.id_produto=@id", Db.P("@id", id));
            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new Produto
            {
                Id = Convert.ToInt32(r["id_produto"]),
                CodigoBarras = r["codigo_barras"].ToString(),
                Nome = r["nome"].ToString(),
                Descricao = r["descricao"].ToString(),
                PrecoCusto = Convert.ToDecimal(r["preco_custo"]),
                PrecoVenda = Convert.ToDecimal(r["preco_venda"]),
                IdCategoria = Convert.ToInt32(r["id_categoria"]),
                Quantidade = Convert.ToInt32(r["qtd"]),
                EstoqueMinimo = Convert.ToInt32(r["minimo"])
            };
        }

        /// <summary>Maior percentual de promoção ativa hoje para o produto (0 se não houver).</summary>
        public decimal PercentualPromocao(int idProduto)
        {
            object o = Db.Escalar(
                @"SELECT COALESCE(MAX(pr.percentual),0)
                  FROM promocao_produto pp JOIN promocoes pr ON pr.id_promocao = pp.id_promocao
                  WHERE pp.id_produto=@id AND pr.ativa=1 AND CURDATE() BETWEEN pr.data_inicio AND pr.data_fim",
                Db.P("@id", idProduto));
            return Convert.ToDecimal(o);
        }

        /// <summary>Insere o produto e sua linha de estoque na MESMA transação.</summary>
        public void Inserir(Produto p)
        {
            using var con = Conexao.Criar();
            con.Open();
            using var tx = con.BeginTransaction();
            try
            {
                var cmd = new MySqlCommand(
                    @"INSERT INTO produtos (codigo_barras, nome, descricao, preco_custo, preco_venda, id_categoria)
                      VALUES (@cb,@n,@d,@pc,@pv,@cat)", con, tx);
                cmd.Parameters.AddWithValue("@cb", string.IsNullOrWhiteSpace(p.CodigoBarras) ? (object)DBNull.Value : p.CodigoBarras);
                cmd.Parameters.AddWithValue("@n", p.Nome);
                cmd.Parameters.AddWithValue("@d", p.Descricao ?? "");
                cmd.Parameters.AddWithValue("@pc", p.PrecoCusto);
                cmd.Parameters.AddWithValue("@pv", p.PrecoVenda);
                cmd.Parameters.AddWithValue("@cat", p.IdCategoria);
                cmd.ExecuteNonQuery();
                long idNovo = cmd.LastInsertedId;

                var ce = new MySqlCommand(
                    "INSERT INTO estoque (id_produto, quantidade, estoque_minimo) VALUES (@id,@q,@m)", con, tx);
                ce.Parameters.AddWithValue("@id", idNovo);
                ce.Parameters.AddWithValue("@q", p.Quantidade);
                ce.Parameters.AddWithValue("@m", p.EstoqueMinimo);
                ce.ExecuteNonQuery();

                if (p.Quantidade > 0)
                {
                    var cm = new MySqlCommand(
                        @"INSERT INTO movimentacoes_estoque (id_produto, tipo, quantidade, motivo, id_usuario)
                          VALUES (@id,'ENTRADA',@q,'Estoque inicial',@u)", con, tx);
                    cm.Parameters.AddWithValue("@id", idNovo);
                    cm.Parameters.AddWithValue("@q", p.Quantidade);
                    cm.Parameters.AddWithValue("@u", Sessao.IdUsuario);
                    cm.ExecuteNonQuery();
                }
                tx.Commit();
            }
            catch { tx.Rollback(); throw; }
        }

        /// <summary>Atualiza dados do produto e o estoque mínimo (saldo só muda por movimentação).</summary>
        public void Atualizar(Produto p)
        {
            using var con = Conexao.Criar();
            con.Open();
            using var tx = con.BeginTransaction();
            try
            {
                var cmd = new MySqlCommand(
                    @"UPDATE produtos SET codigo_barras=@cb, nome=@n, descricao=@d, preco_custo=@pc,
                             preco_venda=@pv, id_categoria=@cat WHERE id_produto=@id", con, tx);
                cmd.Parameters.AddWithValue("@cb", string.IsNullOrWhiteSpace(p.CodigoBarras) ? (object)DBNull.Value : p.CodigoBarras);
                cmd.Parameters.AddWithValue("@n", p.Nome);
                cmd.Parameters.AddWithValue("@d", p.Descricao ?? "");
                cmd.Parameters.AddWithValue("@pc", p.PrecoCusto);
                cmd.Parameters.AddWithValue("@pv", p.PrecoVenda);
                cmd.Parameters.AddWithValue("@cat", p.IdCategoria);
                cmd.Parameters.AddWithValue("@id", p.Id);
                cmd.ExecuteNonQuery();

                var ce = new MySqlCommand("UPDATE estoque SET estoque_minimo=@m WHERE id_produto=@id", con, tx);
                ce.Parameters.AddWithValue("@m", p.EstoqueMinimo);
                ce.Parameters.AddWithValue("@id", p.Id);
                ce.ExecuteNonQuery();
                tx.Commit();
            }
            catch { tx.Rollback(); throw; }
        }

        /// <summary>Exclusão lógica: preserva o histórico de vendas do produto.</summary>
        public void Inativar(int id) =>
            Db.Executar("UPDATE produtos SET ativo=0 WHERE id_produto=@id", Db.P("@id", id));
    }
}
