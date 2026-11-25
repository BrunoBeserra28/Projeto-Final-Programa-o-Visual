using System.Data;
using PapelariaApp.Data;

namespace PapelariaApp.DAO
{
    /// <summary>Consultas de leitura: dashboard, histórico e relatório de vendas.</summary>
    public class RelatorioDAO
    {
        // 'ate' é inclusivo: a consulta usa < ate + 1 dia
        public DataTable Historico(DateTime de, DateTime ate) => Db.Consultar(
            @"SELECT id_venda AS Venda, data_venda AS Data, cliente AS Cliente, vendedor AS Vendedor,
                     forma_pagto AS Pagamento, valor_total AS Total
              FROM vw_relatorio_vendas
              WHERE data_venda >= @de AND data_venda < @ate
              ORDER BY data_venda DESC",
            Db.P("@de", de.Date), Db.P("@ate", ate.Date.AddDays(1)));

        public DataTable ItensDaVenda(int idVenda) => Db.Consultar(
            @"SELECT p.nome AS Produto, iv.quantidade AS Qtd, iv.preco_unitario AS Preco,
                     iv.desconto AS Desconto, (iv.quantidade*iv.preco_unitario - iv.desconto) AS Subtotal
              FROM itens_venda iv JOIN produtos p ON p.id_produto = iv.id_produto
              WHERE iv.id_venda=@id", Db.P("@id", idVenda));

        public DataTable TopProdutos(DateTime de, DateTime ate) => Db.Consultar(
            @"SELECT p.nome AS Produto, SUM(iv.quantidade) AS Quantidade,
                     SUM(iv.quantidade*iv.preco_unitario - iv.desconto) AS Total
              FROM itens_venda iv
              JOIN vendas v   ON v.id_venda = iv.id_venda
              JOIN produtos p ON p.id_produto = iv.id_produto
              WHERE v.data_venda >= @de AND v.data_venda < @ate
              GROUP BY p.id_produto, p.nome
              ORDER BY Quantidade DESC LIMIT 10",
            Db.P("@de", de.Date), Db.P("@ate", ate.Date.AddDays(1)));

        /// <summary>Quantidade de vendas e valor total no período.</summary>
        public (int qtd, decimal total) Resumo(DateTime de, DateTime ate)
        {
            var dt = Db.Consultar(
                @"SELECT COUNT(*) AS q, COALESCE(SUM(valor_total),0) AS t FROM vendas
                  WHERE data_venda >= @de AND data_venda < @ate",
                Db.P("@de", de.Date), Db.P("@ate", ate.Date.AddDays(1)));
            return (Convert.ToInt32(dt.Rows[0]["q"]), Convert.ToDecimal(dt.Rows[0]["t"]));
        }

        // ---------- Dashboard ----------
        public int ContarClientes() => Convert.ToInt32(Db.Escalar("SELECT COUNT(*) FROM clientes"));
        public int ContarEstoqueBaixo() => Convert.ToInt32(Db.Escalar("SELECT COUNT(*) FROM vw_estoque_baixo"));
        public DataTable EstoqueBaixo() => Db.Consultar(
            "SELECT nome AS Produto, categoria AS Categoria, quantidade AS Quantidade, estoque_minimo AS Minimo FROM vw_estoque_baixo ORDER BY quantidade");
    }
}
