using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PapelariaApp.Data;
using PapelariaApp.DAO;

namespace PapelariaApp.Forms
{
    /// <summary>Controle de estoque: consulta, filtros, estoque baixo e movimentação (entrada/saída).</summary>
    public class FormEstoque : Form
    {
        private readonly EstoqueDAO dao = new EstoqueDAO();
        private readonly CategoriaDAO catDao = new CategoriaDAO();
        private readonly ProdutoDAO prodDao = new ProdutoDAO();
        private TextBox txtBusca, txtMotivo;
        private ComboBox cmbCat, cmbProduto, cmbTipo;
        private CheckBox chkBaixo;
        private NumericUpDown numQtd, numMin;
        private DataGridView gridEstoque, gridMov;

        public FormEstoque()
        {
            Text = "Controle de Estoque";
            ClientSize = new Size(950, 640);
            UI.Tema(this);

            // --- filtros ---
            txtBusca = UI.Txt(80, 10, 200);
            cmbCat = UI.Combo(370, 10, 160);
            var cats = catDao.ListarCombo();
            var l = cats.NewRow(); l["Id"] = 0; l["Nome"] = "(Todas)"; cats.Rows.InsertAt(l, 0);
            UI.Preencher(cmbCat, cats, "Nome");
            chkBaixo = new CheckBox { Text = "Somente estoque baixo", Left = 550, Top = 12, AutoSize = true };
            Controls.AddRange(new Control[]
            {
                UI.Lbl("Produto:", 10, 10), txtBusca, UI.Lbl("Categoria:", 295, 10), cmbCat, chkBaixo,
                UI.Btn("Filtrar", 760, 8, (s, e) => CarregarEstoque())
            });

            gridEstoque = UI.Grid(10, 45, 930, 230);
            gridEstoque.DataBindingComplete += (s, e) => PintarBaixos();
            gridEstoque.SelectionChanged += (s, e) =>
            {
                if (gridEstoque.CurrentRow == null || !gridEstoque.CurrentRow.Selected) return;
                int id = Convert.ToInt32(gridEstoque.CurrentRow.Cells["Id"].Value);
                cmbProduto.SelectedValue = id;
                numMin.Value = Convert.ToInt32(gridEstoque.CurrentRow.Cells["Minimo"].Value);
            };
            Controls.Add(gridEstoque);

            // --- movimentação ---
            var box = new GroupBox { Text = "Movimentação de estoque", Left = 10, Top = 285, Width = 930, Height = 100 };
            cmbProduto = UI.Combo(70, 25, 300);
            UI.Preencher(cmbProduto, prodDao.ListarCombo(), "Nome");
            cmbTipo = UI.Combo(430, 25, 100);
            cmbTipo.Items.AddRange(new object[] { "ENTRADA", "SAIDA" });
            cmbTipo.SelectedIndex = 0;
            numQtd = UI.Num(600, 25, 70, 100000);
            txtMotivo = UI.Txt(740, 25, 175);
            numMin = UI.Num(150, 62, 70, 100000);
            box.Controls.AddRange(new Control[]
            {
                UI.Lbl("Produto", 10, 25), cmbProduto, UI.Lbl("Tipo", 390, 25), cmbTipo,
                UI.Lbl("Qtd", 560, 25), numQtd, UI.Lbl("Motivo*", 685, 25), txtMotivo,
                UI.Lbl("Estoque mínimo", 10, 62), numMin,
                UI.Btn("Salvar mínimo", 235, 60, SalvarMinimo, 110),
                UI.Btn("Registrar movimentação", 700, 60, Registrar, 215)
            });
            Controls.Add(box);

            Controls.Add(new Label { Text = "Últimas movimentações", Left = 10, Top = 395, AutoSize = true, Font = new Font("Segoe UI", 10f, FontStyle.Bold) });
            gridMov = UI.Grid(10, 420, 930, 210);
            Controls.Add(gridMov);

            Load += (s, e) => { CarregarEstoque(); CarregarMov(); };
        }

        private void CarregarEstoque()
        {
            try { gridEstoque.DataSource = dao.Listar(txtBusca.Text.Trim(), UI.Id(cmbCat), chkBaixo.Checked); }
            catch (Exception ex) { UI.Erro(Db.Mensagem(ex)); }
        }

        private void CarregarMov()
        {
            try { gridMov.DataSource = dao.ListarMovimentacoes(200); }
            catch (Exception ex) { UI.Erro(Db.Mensagem(ex)); }
        }

        private void PintarBaixos()
        {
            foreach (DataGridViewRow r in gridEstoque.Rows)
                if (UI.S(r.Cells["Situacao"].Value) == "BAIXO")
                    r.DefaultCellStyle.BackColor = Color.MistyRose;
        }

        private void Registrar(object s, EventArgs e)
        {
            if (UI.Id(cmbProduto) == 0) { UI.Aviso("Selecione um produto."); return; }
            if (numQtd.Value <= 0) { UI.Aviso("Informe uma quantidade maior que zero."); numQtd.Focus(); return; }
            if (!UI.Obrigatorios((txtMotivo, "Motivo"))) return;
            try
            {
                string tipo = cmbTipo.SelectedItem.ToString();
                dao.Movimentar(UI.Id(cmbProduto), tipo, (int)numQtd.Value, txtMotivo.Text.Trim());
                AuditoriaDAO.Registrar("MOVIMENTACAO", $"{tipo} de {numQtd.Value} un. do produto {UI.Id(cmbProduto)}");
                UI.Info("Movimentação registrada.");
                numQtd.Value = 0; txtMotivo.Clear();
                CarregarEstoque(); CarregarMov();
            }
            catch (InvalidOperationException ex) { UI.Aviso(ex.Message); }
            catch (Exception ex) { UI.Erro(Db.Mensagem(ex)); }
        }

        private void SalvarMinimo(object s, EventArgs e)
        {
            if (UI.Id(cmbProduto) == 0) { UI.Aviso("Selecione um produto."); return; }
            try
            {
                dao.DefinirMinimo(UI.Id(cmbProduto), (int)numMin.Value);
                CarregarEstoque();
            }
            catch (Exception ex) { UI.Erro(Db.Mensagem(ex)); }
        }
    }
}
