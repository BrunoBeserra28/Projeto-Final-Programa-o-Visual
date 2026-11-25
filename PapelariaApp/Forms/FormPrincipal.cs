using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PapelariaApp.Data;
using PapelariaApp.DAO;

namespace PapelariaApp.Forms
{
    /// <summary>Tela principal: menu + dashboard com indicadores e alerta de estoque baixo.</summary>
    public class FormPrincipal : Form
    {
        private readonly RelatorioDAO rel = new RelatorioDAO();
        private Label lblHoje, lblMes, lblBaixo, lblClientes;
        private DataGridView gridBaixo;
        public bool Deslogou { get; private set; }

        public FormPrincipal()
        {
            Text = $"Papelaria - {Sessao.Atual.Nome} ({Sessao.Atual.Perfil})";
            ClientSize = new Size(960, 600);
            UI.Tema(this);
            MontarMenu();
            MontarDashboard();
            Activated += (s, e) => AtualizarDashboard();
        }

        private void MontarMenu()
        {
            string p = Sessao.Atual.Perfil;
            bool admin = p == "ADMIN", vend = p == "VENDEDOR" || admin, estq = p == "ESTOQUISTA" || admin;

            var menu = new MenuStrip();
            var cad = new ToolStripMenuItem("Cadastros");
            cad.DropDownItems.Add(Item("Clientes", () => new FormClientes(), vend));
            cad.DropDownItems.Add(Item("Produtos", () => new FormProdutos(), estq));
            cad.DropDownItems.Add(Item("Fornecedores", () => new FormFornecedores(), estq));
            cad.DropDownItems.Add(Item("Categorias", () => new FormCategorias(), estq));
            cad.DropDownItems.Add(Item("Usuários", () => new FormUsuarios(), admin));

            var est = new ToolStripMenuItem("Estoque");
            est.DropDownItems.Add(Item("Controle e movimentação", () => new FormEstoque(), estq));

            var ven = new ToolStripMenuItem("Vendas");
            ven.DropDownItems.Add(Item("Nova venda", () => new FormVenda(), vend));
            ven.DropDownItems.Add(Item("Histórico e relatório", () => new FormHistorico(), vend));

            var sair = new ToolStripMenuItem("Sair");
            sair.DropDownItems.Add("Trocar de usuário", null, (s, e) => { Deslogou = true; Close(); });
            sair.DropDownItems.Add("Encerrar", null, (s, e) => Close());

            menu.Items.AddRange(new ToolStripItem[] { cad, est, ven, sair });
            MainMenuStrip = menu;
            Controls.Add(menu);
        }

        /// <summary>Cria item de menu que abre um formulário; desabilitado conforme o perfil (permissões).</summary>
        private ToolStripMenuItem Item(string texto, Func<Form> criar, bool permitido)
        {
            var it = new ToolStripMenuItem(texto) { Enabled = permitido };
            it.Click += (s, e) =>
            {
                using var f = criar();
                f.ShowDialog(this);
                AtualizarDashboard();
            };
            return it;
        }

        private Label Card(string titulo, int x, Color cor)
        {
            var pnl = new Panel { Left = x, Top = 50, Width = 215, Height = 90, BackColor = cor };
            pnl.Controls.Add(new Label { Text = titulo, ForeColor = Color.White, Left = 10, Top = 8, AutoSize = true });
            var valor = new Label
            {
                Text = "-", ForeColor = Color.White, Left = 10, Top = 35, AutoSize = true,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold)
            };
            pnl.Controls.Add(valor);
            Controls.Add(pnl);
            return valor;
        }

        private void MontarDashboard()
        {
            lblHoje = Card("Vendas hoje", 20, Color.FromArgb(30, 130, 90));
            lblMes = Card("Vendas no mês", 250, Color.FromArgb(30, 90, 160));
            lblBaixo = Card("Produtos com estoque baixo", 480, Color.FromArgb(200, 90, 40));
            lblClientes = Card("Clientes cadastrados", 710, Color.FromArgb(110, 70, 160));

            Controls.Add(new Label
            {
                Text = "Produtos com estoque baixo (reposição necessária)",
                Left = 20, Top = 160, AutoSize = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold)
            });
            gridBaixo = UI.Grid(20, 190, 920, 390);
            gridBaixo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(gridBaixo);
        }

        private void AtualizarDashboard()
        {
            try
            {
                var hoje = DateTime.Today;
                var (qh, th) = rel.Resumo(hoje, hoje);
                var (qm, tm) = rel.Resumo(new DateTime(hoje.Year, hoje.Month, 1), hoje);
                lblHoje.Text = $"{th:C2}  ({qh})";
                lblMes.Text = $"{tm:C2}  ({qm})";
                lblBaixo.Text = rel.ContarEstoqueBaixo().ToString();
                lblClientes.Text = rel.ContarClientes().ToString();
                gridBaixo.DataSource = rel.EstoqueBaixo();
            }
            catch (Exception ex) { UI.Erro(Db.Mensagem(ex)); }
        }
    }
}
