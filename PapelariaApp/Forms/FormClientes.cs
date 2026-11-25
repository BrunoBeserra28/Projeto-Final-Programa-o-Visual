using System.Data;
using System.Windows.Forms;
using PapelariaApp.Data;
using PapelariaApp.DAO;
using PapelariaApp.Model;

namespace PapelariaApp.Forms
{
    public class FormClientes : FormCadastroBase
    {
        private readonly ClienteDAO dao = new ClienteDAO();
        private TextBox txtNome, txtCpf, txtTel, txtEmail, txtEnd, txtCidade, txtUf;

        public FormClientes() : base("Clientes", 120)
        {
            pnlCampos.Controls.AddRange(new Control[]
            {
                UI.Lbl("Nome*", 0, 5),     txtNome = UI.Txt(70, 5, 330),
                UI.Lbl("CPF*", 420, 5),    txtCpf = UI.Txt(470, 5, 140),
                UI.Lbl("Telefone", 630, 5),txtTel = UI.Txt(700, 5, 140),
                UI.Lbl("E-mail", 0, 40),   txtEmail = UI.Txt(70, 40, 330),
                UI.Lbl("Endereço", 420, 40), txtEnd = UI.Txt(490, 40, 350),
                UI.Lbl("Cidade", 0, 75),   txtCidade = UI.Txt(70, 75, 200),
                UI.Lbl("UF", 290, 75),     txtUf = UI.Txt(320, 75, 40)
            });
            txtUf.MaxLength = 2;
        }

        protected override DataTable Consultar(string filtro) => dao.Listar(filtro);

        protected override void PreencherCampos(DataGridViewRow r)
        {
            txtNome.Text = UI.S(r.Cells["Nome"].Value);
            txtCpf.Text = UI.S(r.Cells["CPF"].Value);
            txtTel.Text = UI.S(r.Cells["Telefone"].Value);
            txtEmail.Text = UI.S(r.Cells["Email"].Value);
            txtEnd.Text = UI.S(r.Cells["Endereco"].Value);
            txtCidade.Text = UI.S(r.Cells["Cidade"].Value);
            txtUf.Text = UI.S(r.Cells["UF"].Value);
        }

        protected override void LimparCampos()
        {
            foreach (Control c in pnlCampos.Controls) if (c is TextBox t) t.Clear();
            txtNome.Focus();
        }

        protected override bool Salvar()
        {
            if (!UI.Obrigatorios((txtNome, "Nome"), (txtCpf, "CPF"))) return false;
            if (!Validador.QtdDigitos(txtCpf.Text, 11)) { UI.Aviso("O CPF deve ter 11 dígitos."); txtCpf.Focus(); return false; }
            if (txtEmail.Text.Trim().Length > 0 && !Validador.EmailValido(txtEmail.Text.Trim()))
            { UI.Aviso("E-mail inválido."); txtEmail.Focus(); return false; }

            var c = new Cliente
            {
                Id = idAtual, Nome = txtNome.Text.Trim(), Cpf = txtCpf.Text.Trim(), Telefone = txtTel.Text.Trim(),
                Email = txtEmail.Text.Trim(), Endereco = txtEnd.Text.Trim(), Cidade = txtCidade.Text.Trim(),
                Uf = txtUf.Text.Trim().ToUpper()
            };
            if (idAtual == 0) dao.Inserir(c); else dao.Atualizar(c);
            return true;
        }

        protected override void Remover(int id) => dao.Excluir(id);
    }
}
