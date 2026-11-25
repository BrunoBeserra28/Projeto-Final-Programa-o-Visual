using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PapelariaApp.Data;
using PapelariaApp.DAO;

namespace PapelariaApp.Forms
{
    /// <summary>Histórico de vendas por período + relatório (resumo, itens e ranking) com exportação CSV.</summary>
    public class FormHistorico : Form
    {
        private readonly RelatorioDAO dao = new RelatorioDAO();
        private DateTimePicker dtpDe, dtpAte;
        private DataGridView gridVendas, gridItens, gridTop;
        private Label lblResumo;

        public FormHistorico()
        {
            Text = "Histórico e Relatório de Vendas";
            ClientSize = new Size(950, 620);
            UI.Tema(this);

            var hoje = DateTime.Today;
            dtpDe = new DateTimePicker { Left = 40, Top = 10, Width = 110, Format = DateTimePickerFormat.Short, Value = new DateTime(hoje.Year, hoje.Month, 1) };
            dtpAte = new DateTimePicker { Left = 190, Top = 10, Width = 110, Format = DateTimePickerFormat.Short, Value = hoje };
            Controls.AddRange(new Control[]
            {
                UI.Lbl("De", 10, 10), dtpDe, UI.Lbl("Até", 160, 10), dtpAte,
                UI.Btn("Filtrar", 320, 8, (s, e) => Carregar()),
                UI.Btn("Exportar CSV", 420, 8, (s, e) => Exportar(), 120)
            });

            lblResumo = new Label { Left = 10, Top = 45, AutoSize = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = UI.Primaria };
            Controls.Add(lblResumo);

            gridVendas = UI.Grid(10, 75, 930, 220);
            gridVendas.SelectionChanged += (s, e) => CarregarItens();
            Controls.Add(gridVendas);

            Controls.Add(new Label { Text = "Itens da venda selecionada", Left = 10, Top = 305, AutoSize = true, Font = new Font("Segoe UI", 10f, FontStyle.Bold) });
            gridItens = UI.Grid(10, 330, 460, 280);
            Controls.Add(gridItens);

            Controls.Add(new Label { Text = "Produtos mais vendidos no período", Left = 485, Top = 305, AutoSize = true, Font = new Font("Segoe UI", 10f, FontStyle.Bold) });
            gridTop = UI.Grid(485, 330, 455, 280);
            Controls.Add(gridTop);

            Load += (s, e) => Carregar();
        }

        private void Carregar()
        {
            if (dtpAte.Value.Date < dtpDe.Value.Date) { UI.Aviso("A data final não pode ser anterior à inicial."); return; }
            try
            {
                gridVendas.DataSource = dao.Historico(dtpDe.Value, dtpAte.Value);
                if (gridVendas.Columns["Total"] != null) gridVendas.Columns["Total"].DefaultCellStyle.Format = "C2";
                if (gridVendas.Columns["Data"] != null) gridVendas.Columns["Data"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

                gridTop.DataSource = dao.TopProdutos(dtpDe.Value, dtpAte.Value);
                if (gridTop.Columns["Total"] != null) gridTop.Columns["Total"].DefaultCellStyle.Format = "C2";

                var (qtd, total) = dao.Resumo(dtpDe.Value, dtpAte.Value);
                decimal ticket = qtd == 0 ? 0 : total / qtd;
                lblResumo.Text = $"Vendas: {qtd}   |   Total: {total:C2}   |   Ticket médio: {ticket:C2}";
                CarregarItens();
            }
            catch (Exception ex) { UI.Erro(Db.Mensagem(ex)); }
        }

        private void CarregarItens()
        {
            if (gridVendas.CurrentRow == null || !gridVendas.CurrentRow.Selected) { gridItens.DataSource = null; return; }
            try
            {
                int id = Convert.ToInt32(gridVendas.CurrentRow.Cells["Venda"].Value);
                gridItens.DataSource = dao.ItensDaVenda(id);
                foreach (var c in new[] { "Preco", "Desconto", "Subtotal" })
                    if (gridItens.Columns[c] != null) gridItens.Columns[c].DefaultCellStyle.Format = "C2";
            }
            catch (Exception ex) { UI.Erro(Db.Mensagem(ex)); }
        }

        /// <summary>Exporta a grade de vendas para CSV (abre no Excel, separador ';').</summary>
        private void Exportar()
        {
            if (gridVendas.DataSource is not DataTable dt || dt.Rows.Count == 0)
            { UI.Aviso("Não há dados para exportar."); return; }

            using var dlg = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "relatorio_vendas.csv" };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine(string.Join(";", dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));
                foreach (DataRow r in dt.Rows)
                    sb.AppendLine(string.Join(";", r.ItemArray.Select(v => "\"" + UI.S(v).Replace("\"", "\"\"") + "\"")));
                File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
                UI.Info("Arquivo exportado com sucesso.");
            }
            catch (Exception ex) { UI.Erro("Falha ao exportar: " + ex.Message); }
        }
    }
}
