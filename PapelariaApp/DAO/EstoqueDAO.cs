using System.Data;
using MySqlConnector;
using PapelariaApp.Data;

namespace PapelariaApp.DAO
{
    public class EstoqueDAO
    {
        public DataTable Listar(string filtro, int idCategoria, bool apenasBaixo) => Db.Consultar(
            @"SELECT p.id_produto AS Id, p.nome AS Produto, c.nome AS Categoria,
                     e.quantidade AS Quantidade, e.estoque_minimo AS Minimo,
                     IF(e.quantidade <= e.estoque_minimo, 'BAIXO', 'OK') AS Situacao
              FROM estoque e
              JOIN produtos p   ON p.id_produto = e.id_produto
              JOIN categorias c ON c.id_categoria = p.id_categoria
              WHERE p.ativo = 1
                AND p.nome LIKE @f
                AND (@cat = 0 OR p.id_categoria = @cat)
                AND (@baixo = 0 OR e.quantidade <= e.estoque_minimo)
              ORDER BY p.nome",
            Db.P("@f", "%" + filtro + "%"), Db.P("@cat", idCategoria), Db.P("@baixo", apenasBaixo ? 1 : 0));

        public DataTable ListarMovimentacoes(int limite) => Db.Consultar(
            @"SELECT m.id_movimentacao AS Id, m.data_mov AS Data, p.nome AS Produto, m.tipo AS Tipo,
                     m.quantidade AS Quantidade, m.motivo AS Motivo, u.nome AS Usuario
              FROM movimentacoes_estoque m
              JOIN produtos p ON p.id_produto = m.id_produto
              JOIN usuarios u ON u.id_usuario = m.id_usuario
              ORDER BY m.data_mov DESC, m.id_movimentacao DESC
              LIMIT @lim", Db.P("@lim", limite));

        public void DefinirMinimo(int idProduto, int minimo) =>
            Db.Executar("UPDATE estoque SET estoque_minimo=@m WHERE id_produto=@id",
                Db.P("@m", minimo), Db.P("@id", idProduto));

        /// <summary>
        /// Entrada/saída manual de estoque. Operação crítica: usa transação e trava a linha
        /// (SELECT ... FOR UPDATE) para impedir saldo negativo em acessos simultâneos.
        /// </summary>
        public void Movimentar(int idProduto, string tipo, int quantidade, string motivo)
        {
            if (quantidade <= 0) throw new InvalidOperationException("A quantidade deve ser maior que zero.");
            using var con = Conexao.Criar();
            con.Open();
            using var tx = con.BeginTransaction();
            try
            {
                var sel = new MySqlCommand("SELECT quantidade FROM estoque WHERE id_produto=@id FOR UPDATE", con, tx);
                sel.Parameters.AddWithValue("@id", idProduto);
                object o = sel.ExecuteScalar();
                if (o == null) throw new InvalidOperationException("Produto sem registro de estoque.");
                int atual = Convert.ToInt32(o);
                int novo = tipo == "ENTRADA" ? atual + quantidade : atual - quantidade;
                if (novo < 0)
                    throw new InvalidOperationException($"Saldo insuficiente. Disponível: {atual}.");

                var up = new MySqlCommand("UPDATE estoque SET quantidade=@q WHERE id_produto=@id", con, tx);
                up.Parameters.AddWithValue("@q", novo);
                up.Parameters.AddWithValue("@id", idProduto);
                up.ExecuteNonQuery();

                var mov = new MySqlCommand(
                    @"INSERT INTO movimentacoes_estoque (id_produto, tipo, quantidade, motivo, id_usuario)
                      VALUES (@id,@t,@q,@m,@u)", con, tx);
                mov.Parameters.AddWithValue("@id", idProduto);
                mov.Parameters.AddWithValue("@t", tipo);
                mov.Parameters.AddWithValue("@q", quantidade);
                mov.Parameters.AddWithValue("@m", motivo);
                mov.Parameters.AddWithValue("@u", Sessao.IdUsuario);
                mov.ExecuteNonQuery();
                tx.Commit();
            }
            catch { tx.Rollback(); throw; }
        }
    }
}
