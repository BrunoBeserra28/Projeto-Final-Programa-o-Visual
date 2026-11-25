using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using PapelariaApp.Data;
using PapelariaApp.DAO;
using PapelariaApp.Model;

namespace PapelariaApp.Forms
{
    /// <summary>Registro de venda: monta o carrinho, calcula o total e grava com baixa de estoque.</summary>
    public class FormVenda : Form
    {
        private readonly ProdutoDAO prodDao = new ProdutoDAO();
        private readonly ClienteDAO cliDao = new ClienteDAO();
        private readonly VendaDAO vendaDao = new VendaDAO();
        private readonly BindingList<ItemVenda> itens = new BindingList<ItemVenda>();

        private ComboBox cmbCliente, cmbPagto, cmbProduto;
        private TextBox txtBuscaProd;
        private NumericUpDown numQtd;
        private DataGridView grid;
        private Label lblTotal;

        public FormVenda()
        {
            Text = "Nova Venda";
            ClientSize = new Size(950, 600);
            UI.Tema(this);

            cmbCliente = UI.Combo(80, 10, 300);
            UI.Preencher(cmbCliente, cliDao.ListarCombo(), "Nome");
            cmbPagto = UI.Combo(520, 10, 160);
            cmbPagto.Items.AddRange(new object[] { "DINHEIRO", "CARTAO_DEBITO", "CARTAO_CREDITO", "PIX" });
            cmbPagto.SelectedIndex = 0;

            txtBuscaProd = UI.Txt(80, 55, 200);
            txtBuscaProd.TextChanged += (s, e) => CarregarProdutos();
            cmbProduto = UI.Combo(80, 90, 480);
            numQtd = UI.Num(620, 90, 70, 10000);
            numQtd.Minimum = 1; numQtd.Value = 1;

            Controls.AddRange(new Control[]
            {
                UI.Lbl("Cliente", 10, 10), cmbCliente, UI.Lbl("Pagamento", 440, 10), cmbPagto,
                UI.Lbl("Buscar", 10, 55), txtBuscaProd,
                UI.Lbl("Produto", 10, 90), cmbProduto, UI.Lbl("Qtd", 575, 90), numQtd,
                UI.Btn("Adicionar", 710, 88, (s, e) => Adicionar(), 100),
                UI.Btn("Remover item", 820, 88, (s, e) => Remover(), 110)
            });

            grid = UI.Grid(10, 130, 930, 370);
            grid.DataSource = itens;
            grid.DataBindingComplete += (s, e) => FormatarGrade();
            Controls.Add(grid);

            lblTotal = new Label
            {
                Text = "Total: R$ 0,00", Left = 10, Top = 515, AutoSize = true,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = UI.Primaria
            };
            Controls.Add(lblTotal);
            Controls.Add(UI.Btn("Finalizar venda", 740, 515, (s, e) => Finalizar(), 200));

            Load += (s, e) => CarregarProdutos();
        }

        private void FormatarGrade()
        {
            if (grid.Columns["IdProduto"] != null) grid.Columns["IdProduto"].Visible = false;
            Cabecalho("NomeProduto", "Produto"); Cabecalho("Quantidade", "Qtd");
            Cabecalho("PrecoUnitario", "Preço unit."); Cabecalho("Desconto", "Desconto (R$)");
            foreach (var c in new[] { "PrecoUnitario", "Desconto", "Subtotal" })
                if (grid.Columns[c] != null) grid.Columns[c].DefaultCellStyle.Format = "C2";
        }

        private void Cabecalho(string coluna, string texto)
        {
            if (grid.Columns[coluna] != null) grid.Columns[coluna].HeaderText = texto;
        }

        private void CarregarProdutos()
        {
            try { UI.Preencher(cmbProduto, prodDao.ListarParaVenda(txtBuscaProd.Text.Trim()), "Texto"); }
            catch (Exception ex) { UI.Erro(Db.Mensagem(ex)); }
        }

        private void Adicionar()
        {
            try
            {
                int id = UI.Id(cmbProduto);
                if (id == 0) { UI.Aviso("Selecione um produto."); return; }
                var p = prodDao.ObterPorId(id);
                int qtd = (int)numQtd.Value;

                var existente = itens.FirstOrDefault(i => i.IdProduto == id);
                int noCarrinho = existente?.Quantidade ?? 0;
                if (noCarrinho + qtd > p.Quantidade)
                {
                    UI.Aviso($"Estoque insuficiente. Disponível: {p.Quantidade} (já no carrinho: {noCarrinho}).");
                    return;
                }

                // desconto automático se houver promoção ativa para o produto
                decimal perc = prodDao.PercentualPromocao(id);
                decimal descontoUnit = Math.Round(p.PrecoVenda * perc / 100m, 2);

                if (existente != null)
                {
                    existente.Quantidade += qtd;
                    existente.Desconto = descontoUnit * existente.Quantidade;
                }
                else
                {
                    itens.Add(new ItemVenda
                    {
                        IdProduto = id, NomeProduto = p.Nome, Quantidade = qtd,
                        PrecoUnitario = p.PrecoVenda, Desconto = descontoUnit * qtd
                    });
                }
                itens.ResetBindings();
                AtualizarTotal();
                numQtd.Value = 1;
            }
            catch (Exception ex) { UI.Erro(Db.Mensagem(ex)); }
        }

        private void Remover()
        {
            if (grid.CurrentRow == null) { UI.Aviso("Selecione um item para remover."); return; }
            itens.RemoveAt(grid.CurrentRow.Index);
            AtualizarTotal();
        }

        private void AtualizarTotal() =>
            lblTotal.Text = "Total: " + itens.Sum(i => i.Subtotal).ToString("C2");

        private void Finalizar()
        {
            if (itens.Count == 0) { UI.Aviso("Adicione ao menos um produto à venda."); return; }
            if (!UI.Confirma($"Confirmar a venda no valor de {itens.Sum(i => i.Subtotal):C2}?")) return;

            try
            {
                var v = new Venda
                {
                    IdCliente = UI.Id(cmbCliente) == 0 ? (int?)null : UI.Id(cmbCliente),
                    IdUsuario = Sessao.IdUsuario,
                    FormaPagamento = cmbPagto.SelectedItem.ToString(),
                    Itens = itens.ToList()
                };
                int id = vendaDao.Registrar(v);
                AuditoriaDAO.Registrar("VENDA", $"Venda #{id} - {v.Total:C2}");
                UI.Info($"Venda #{id} registrada com sucesso!\nTotal: {v.Total:C2}");
                itens.Clear();
                AtualizarTotal();
                CarregarProdutos();   // saldos mudaram
            }
            catch (InvalidOperationException ex) { UI.Aviso(ex.Message); CarregarProdutos(); }
            catch (Exception ex) { UI.Erro(Db.Mensagem(ex)); }
        }
    }
}
