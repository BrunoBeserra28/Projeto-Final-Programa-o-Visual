using MySqlConnector;
using PapelariaApp.Data;
using PapelariaApp.Model;

namespace PapelariaApp.DAO
{
    public class VendaDAO
    {
        /// <summary>
        /// Registra a venda inteira em UMA transação:
        /// 1) grava a venda; 2) grava os itens; 3) dá baixa no estoque; 4) registra a movimentação.
        /// Se qualquer passo falhar (ex.: estoque insuficiente), tudo é desfeito (ROLLBACK).
        /// </summary>
        public int Registrar(Venda v)
        {
            if (v.Itens.Count == 0)
                throw new InvalidOperationException("A venda precisa ter ao menos um item.");

            using var con = Conexao.Criar();
            con.Open();
            using var tx = con.BeginTransaction();
            try
            {
                var cv = new MySqlCommand(
                    @"INSERT INTO vendas (id_cliente, id_usuario, forma_pagto, valor_total)
                      VALUES (@c,@u,@f,@t)", con, tx);
                cv.Parameters.AddWithValue("@c", v.IdCliente.HasValue ? (object)v.IdCliente.Value : DBNull.Value);
                cv.Parameters.AddWithValue("@u", v.IdUsuario);
                cv.Parameters.AddWithValue("@f", v.FormaPagamento);
                cv.Parameters.AddWithValue("@t", v.Total);
                cv.ExecuteNonQuery();
                int idVenda = (int)cv.LastInsertedId;

                foreach (var it in v.Itens)
                {
                    // trava a linha de estoque e confere o saldo
                    var sel = new MySqlCommand("SELECT quantidade FROM estoque WHERE id_produto=@p FOR UPDATE", con, tx);
                    sel.Parameters.AddWithValue("@p", it.IdProduto);
                    object o = sel.ExecuteScalar();
                    int saldo = o == null ? 0 : Convert.ToInt32(o);
                    if (saldo < it.Quantidade)
                        throw new InvalidOperationException(
                            $"Estoque insuficiente para \"{it.NomeProduto}\". Disponível: {saldo}.");

                    var ci = new MySqlCommand(
                        @"INSERT INTO itens_venda (id_venda, id_produto, quantidade, preco_unitario, desconto)
                          VALUES (@v,@p,@q,@pu,@d)", con, tx);
                    ci.Parameters.AddWithValue("@v", idVenda);
                    ci.Parameters.AddWithValue("@p", it.IdProduto);
                    ci.Parameters.AddWithValue("@q", it.Quantidade);
                    ci.Parameters.AddWithValue("@pu", it.PrecoUnitario);
                    ci.Parameters.AddWithValue("@d", it.Desconto);
                    ci.ExecuteNonQuery();

                    // baixa automática no estoque
                    var ce = new MySqlCommand("UPDATE estoque SET quantidade = quantidade - @q WHERE id_produto=@p", con, tx);
                    ce.Parameters.AddWithValue("@q", it.Quantidade);
                    ce.Parameters.AddWithValue("@p", it.IdProduto);
                    ce.ExecuteNonQuery();

                    var cm = new MySqlCommand(
                        @"INSERT INTO movimentacoes_estoque (id_produto, tipo, quantidade, motivo, id_usuario, id_venda)
                          VALUES (@p,'SAIDA',@q,@m,@u,@v)", con, tx);
                    cm.Parameters.AddWithValue("@p", it.IdProduto);
                    cm.Parameters.AddWithValue("@q", it.Quantidade);
                    cm.Parameters.AddWithValue("@m", "Venda #" + idVenda);
                    cm.Parameters.AddWithValue("@u", v.IdUsuario);
                    cm.Parameters.AddWithValue("@v", idVenda);
                    cm.ExecuteNonQuery();
                }

                tx.Commit();
                return idVenda;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
