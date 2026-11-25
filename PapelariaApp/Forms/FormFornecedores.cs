using System.Data;
using System.Windows.Forms;
using PapelariaApp.Data;
using PapelariaApp.DAO;
using PapelariaApp.Model;

namespace PapelariaApp.Forms
{
    public class FormFornecedores : FormCadastroBase
    {
        private readonly FornecedorDAO dao = new FornecedorDAO();
        private TextBox txtRazao, txtCnpj, txtTel, txtEmail, txtEnd;

        public FormFornecedores() : base("Fornecedores", 85)
        {
            pnlCampos.Controls.AddRange(new Control[]
            {
                UI.Lbl("Razão social*", 0, 5), txtRazao = UI.Txt(90, 5, 300),
                UI.Lbl("CNPJ*", 420, 5),       txtCnpj = UI.Txt(470, 5, 160),
                UI.Lbl("Telefone", 650, 5),    txtTel = UI.Txt(715, 5, 140),
                UI.Lbl("E-mail", 0, 40),       txtEmail = UI.Txt(90, 40, 300),
                UI.Lbl("Endereço", 420, 40),   txtEnd = UI.Txt(490, 40, 365)
            });
        }

        protected override DataTable Consultar(string filtro) => dao.Listar(filtro);

        protected override void PreencherCampos(DataGridViewRow r)
        {
            txtRazao.Text = UI.S(r.Cells["Razao"].Value);
            txtCnpj.Text = UI.S(r.Cells["CNPJ"].Value);
            txtTel.Text = UI.S(r.Cells["Telefone"].Value);
            txtEmail.Text = UI.S(r.Cells["Email"].Value);
            txtEnd.Text = UI.S(r.Cells["Endereco"].Value);
        }

        protected override void LimparCampos()
        {
            foreach (Control c in pnlCampos.Controls) if (c is TextBox t) t.Clear();
            txtRazao.Focus();
        }

        protected override bool Salvar()
        {
            if (!UI.Obrigatorios((txtRazao, "Razão social"), (txtCnpj, "CNPJ"))) return false;
            if (!Validador.QtdDigitos(txtCnpj.Text, 14)) { UI.Aviso("O CNPJ deve ter 14 dígitos."); txtCnpj.Focus(); return false; }
            if (txtEmail.Text.Trim().Length > 0 && !Validador.EmailValido(txtEmail.Text.Trim()))
            { UI.Aviso("E-mail inválido."); txtEmail.Focus(); return false; }

            var f = new Fornecedor
            {
                Id = idAtual, RazaoSocial = txtRazao.Text.Trim(), Cnpj = txtCnpj.Text.Trim(),
                Telefone = txtTel.Text.Trim(), Email = txtEmail.Text.Trim(), Endereco = txtEnd.Text.Trim()
            };
            if (idAtual == 0) dao.Inserir(f); else dao.Atualizar(f);
            return true;
        }

        protected override void Remover(int id) => dao.Inativar(id);
    }
}
