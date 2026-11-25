using System.Data;
using System.Windows.Forms;
using PapelariaApp.DAO;
using PapelariaApp.Model;

namespace PapelariaApp.Forms
{
    public class FormProdutos : FormCadastroBase
    {
        private readonly ProdutoDAO dao = new ProdutoDAO();
        private readonly CategoriaDAO catDao = new CategoriaDAO();
        private TextBox txtCodigo, txtNome, txtDesc, txtCusto, txtPreco;
        private ComboBox cmbCategoria, cmbFiltroCat;
        private NumericUpDown numEstoque, numMinimo;

        public FormProdutos() : base("Produtos", 120)
        {
            // filtro por categoria na barra superior
            Controls.Add(UI.Lbl("Categoria:", 430, 10));
            cmbFiltroCat = UI.Combo(500, 10, 130);
            var todas = catDao.ListarCombo();
            var linha = todas.NewRow(); linha["Id"] = 0; linha["Nome"] = "(Todas)";
            todas.Rows.InsertAt(linha, 0);
            UI.Preencher(cmbFiltroCat, todas, "Nome");
            Controls.Add(cmbFiltroCat);
            cmbFiltroCat.SelectedIndexChanged += (s, e) => CarregarGrid();

            cmbCategoria = UI.Combo(110, 40, 220);
            UI.Preencher(cmbCategoria, catDao.ListarCombo(), "Nome");
            numEstoque = UI.Num(450, 75, 80);
            numMinimo = UI.Num(650, 75, 80);

            pnlCampos.Controls.AddRange(new Control[]
            {
                UI.Lbl("Cód. barras", 0, 5), txtCodigo = UI.Txt(110, 5, 150),
                UI.Lbl("Nome*", 280, 5),     txtNome = UI.Txt(330, 5, 300),
                UI.Lbl("Descrição", 650, 5), txtDesc = UI.Txt(720, 5, 200),
                UI.Lbl("Categoria*", 0, 40), cmbCategoria,
                UI.Lbl("Preço custo*", 350, 40), txtCusto = UI.Txt(440, 40, 90),
                UI.Lbl("Preço venda*", 560, 40), txtPreco = UI.Txt(650, 40, 90),
                UI.Lbl("Estoque inicial (só em novo produto)", 0, 75), numEstoque,
                UI.Lbl("Estoque mínimo", 560, 75), numMinimo
            });
            numEstoque.Left = 250; numMinimo.Left = 660;
        }

        protected override DataTable Consultar(string filtro) => dao.Listar(filtro, UI.Id(cmbFiltroCat));

        protected override void OcultarColunas()
        {
            foreach (var c in new[] { "Id", "IdCategoria", "Descricao", "Minimo" })
                if (grid.Columns[c] != null) grid.Columns[c].Visible = false;
            if (grid.Columns["Custo"] != null) grid.Columns["Custo"].DefaultCellStyle.Format = "C2";
            if (grid.Columns["Preco"] != null) grid.Columns["Preco"].DefaultCellStyle.Format = "C2";
        }

        protected override void PreencherCampos(DataGridViewRow r)
        {
            txtCodigo.Text = UI.S(r.Cells["Codigo"].Value);
            txtNome.Text = UI.S(r.Cells["Nome"].Value);
            txtDesc.Text = UI.S(r.Cells["Descricao"].Value);
            txtCusto.Text = Convert.ToDecimal(r.Cells["Custo"].Value).ToString("0.00");
            txtPreco.Text = Convert.ToDecimal(r.Cells["Preco"].Value).ToString("0.00");
            cmbCategoria.SelectedValue = Convert.ToInt32(r.Cells["IdCategoria"].Value);
            numMinimo.Value = Convert.ToInt32(r.Cells["Minimo"].Value);
            numEstoque.Value = 0;
            numEstoque.Enabled = false; // saldo só muda por movimentação de estoque
        }

        protected override void LimparCampos()
        {
            foreach (Control c in pnlCampos.Controls) if (c is TextBox t) t.Clear();
            if (cmbCategoria.Items.Count > 0) cmbCategoria.SelectedIndex = 0;
            numEstoque.Value = 0; numEstoque.Enabled = true; numMinimo.Value = 5;
            txtNome.Focus();
        }

        protected override bool Salvar()
        {
            if (!UI.Obrigatorios((txtNome, "Nome"), (txtCusto, "Preço de custo"), (txtPreco, "Preço de venda"))) return false;
            if (UI.Id(cmbCategoria) == 0) { UI.Aviso("Selecione uma categoria."); return false; }

            decimal custo = UI.Dec(txtCusto.Text, out bool okC);
            decimal preco = UI.Dec(txtPreco.Text, out bool okP);
            if (!okC || custo < 0) { UI.Aviso("Preço de custo inválido."); txtCusto.Focus(); return false; }
            if (!okP || preco < 0) { UI.Aviso("Preço de venda inválido."); txtPreco.Focus(); return false; }

            var p = new Produto
            {
                Id = idAtual, CodigoBarras = txtCodigo.Text.Trim(), Nome = txtNome.Text.Trim(),
                Descricao = txtDesc.Text.Trim(), PrecoCusto = custo, PrecoVenda = preco,
                IdCategoria = UI.Id(cmbCategoria), Quantidade = (int)numEstoque.Value,
                EstoqueMinimo = (int)numMinimo.Value
            };
            if (idAtual == 0) dao.Inserir(p); else dao.Atualizar(p);
            return true;
        }

        protected override void Remover(int id) => dao.Inativar(id);
    }
}
