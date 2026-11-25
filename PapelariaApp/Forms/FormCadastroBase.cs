using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PapelariaApp.Data;
using PapelariaApp.DAO;

namespace PapelariaApp.Forms
{
    /// <summary>
    /// Tela-base dos CRUDs: barra de pesquisa, botões Novo/Salvar/Excluir, painel de campos e grade.
    /// Cada cadastro herda desta classe e implementa só o que é específico (reuso via herança).
    /// </summary>
    public abstract class FormCadastroBase : Form
    {
        protected Panel pnlCampos = new Panel();
        protected DataGridView grid;
        protected TextBox txtBusca;
        protected int idAtual = 0;           // 0 = novo registro
        private bool carregando = false;

        protected FormCadastroBase(string titulo, int alturaCampos)
        {
            Text = titulo;
            ClientSize = new Size(950, 620);
            UI.Tema(this);

            Controls.Add(UI.Lbl("Pesquisar:", 10, 10));
            txtBusca = UI.Txt(80, 10, 250);
            txtBusca.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { CarregarGrid(); e.SuppressKeyPress = true; }
            };
            Controls.Add(txtBusca);
            Controls.Add(UI.Btn("Buscar", 340, 8, (s, e) => CarregarGrid()));
            Controls.Add(UI.Btn("Novo", 640, 8, (s, e) => Novo()));
            Controls.Add(UI.Btn("Salvar", 740, 8, BtnSalvar));
            Controls.Add(UI.Btn("Excluir", 840, 8, BtnExcluir));

            pnlCampos.SetBounds(10, 45, 930, alturaCampos);
            Controls.Add(pnlCampos);

            int topoGrid = 55 + alturaCampos;
            grid = UI.Grid(10, topoGrid, 930, 620 - topoGrid - 10);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.SelectionChanged += (s, e) =>
            {
                if (carregando || grid.CurrentRow == null || !grid.CurrentRow.Selected) return;
                idAtual = Convert.ToInt32(grid.CurrentRow.Cells["Id"].Value);
                PreencherCampos(grid.CurrentRow);
            };
            Controls.Add(grid);

            Load += (s, e) => { CarregarGrid(); Novo(); };
        }

        // ----- pontos de extensão (cada cadastro implementa) -----
        protected abstract DataTable Consultar(string filtro);
        protected abstract void PreencherCampos(DataGridViewRow linha);
        protected abstract void LimparCampos();
        /// <summary>Valida e grava (insert se idAtual==0, update caso contrário). Retorna true se gravou.</summary>
        protected abstract bool Salvar();
        protected abstract void Remover(int id);

        protected virtual void OcultarColunas() { }

        protected void CarregarGrid()
        {
            try
            {
                carregando = true;
                grid.DataSource = Consultar(txtBusca.Text.Trim());
                OcultarColunas();
                grid.ClearSelection();
            }
            catch (Exception ex) { UI.Erro(Db.Mensagem(ex)); }
            finally { carregando = false; }
        }

        protected void Novo()
        {
            idAtual = 0;
            grid.ClearSelection();
            LimparCampos();
        }

        private void BtnSalvar(object s, EventArgs e)
        {
            try
            {
                bool novo = idAtual == 0;
                if (!Salvar()) return;
                AuditoriaDAO.Registrar(novo ? "INCLUSAO" : "ALTERACAO", Text);
                UI.Info("Registro salvo com sucesso.");
                CarregarGrid();
                Novo();
            }
            catch (Exception ex) { UI.Erro(Db.Mensagem(ex)); }
        }

        private void BtnExcluir(object s, EventArgs e)
        {
            if (idAtual == 0) { UI.Aviso("Selecione um registro na grade para excluir."); return; }
            if (!UI.Confirma("Deseja realmente excluir o registro selecionado?")) return;
            try
            {
                Remover(idAtual);
                AuditoriaDAO.Registrar("EXCLUSAO", $"{Text} (id {idAtual})");
                CarregarGrid();
                Novo();
            }
            catch (Exception ex) { UI.Erro(Db.Mensagem(ex)); }
        }
    }
}
