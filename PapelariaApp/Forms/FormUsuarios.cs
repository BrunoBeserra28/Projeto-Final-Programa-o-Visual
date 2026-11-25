using System.Data;
using System.Windows.Forms;
using PapelariaApp.Data;
using PapelariaApp.DAO;
using PapelariaApp.Model;

namespace PapelariaApp.Forms
{
    public class FormUsuarios : FormCadastroBase
    {
        private readonly UsuarioDAO dao = new UsuarioDAO();
        private TextBox txtNome, txtLogin, txtSenha;
        private ComboBox cmbPerfil;
        private CheckBox chkAtivo;

        public FormUsuarios() : base("Usuários", 85)
        {
            txtSenha = UI.Txt(470, 5, 160);
            txtSenha.UseSystemPasswordChar = true;
            cmbPerfil = UI.Combo(90, 40, 160);
            cmbPerfil.Items.AddRange(new object[] { "ADMIN", "VENDEDOR", "ESTOQUISTA" });
            chkAtivo = new CheckBox { Text = "Ativo", Left = 290, Top = 42, Checked = true, AutoSize = true };

            pnlCampos.Controls.AddRange(new Control[]
            {
                UI.Lbl("Nome*", 0, 5),   txtNome = UI.Txt(90, 5, 300),
                UI.Lbl("Senha*", 420, 5), txtSenha,
                UI.Lbl("Login*", 650, 5), txtLogin = UI.Txt(700, 5, 130),
                UI.Lbl("Perfil*", 0, 40), cmbPerfil, chkAtivo,
                new Label { Text = "(ao editar, deixe a senha em branco para mantê-la)", Left = 420, Top = 44, AutoSize = true }
            });
        }

        protected override DataTable Consultar(string filtro) => dao.Listar(filtro);

        protected override void PreencherCampos(DataGridViewRow r)
        {
            txtNome.Text = UI.S(r.Cells["Nome"].Value);
            txtLogin.Text = UI.S(r.Cells["Login"].Value);
            cmbPerfil.SelectedItem = UI.S(r.Cells["Perfil"].Value);
            chkAtivo.Checked = UI.S(r.Cells["Ativo"].Value) == "Sim";
            txtSenha.Clear();
        }

        protected override void LimparCampos()
        {
            txtNome.Clear(); txtLogin.Clear(); txtSenha.Clear();
            cmbPerfil.SelectedIndex = 1; chkAtivo.Checked = true;
            txtNome.Focus();
        }

        protected override bool Salvar()
        {
            if (!UI.Obrigatorios((txtNome, "Nome"), (txtLogin, "Login"), (cmbPerfil, "Perfil"))) return false;
            if (idAtual == 0 && !UI.Obrigatorios((txtSenha, "Senha"))) return false;
            if (txtSenha.Text.Length > 0 && txtSenha.Text.Length < 6)
            { UI.Aviso("A senha deve ter pelo menos 6 caracteres."); txtSenha.Focus(); return false; }

            var u = new Usuario
            {
                Id = idAtual, Nome = txtNome.Text.Trim(), Login = txtLogin.Text.Trim(),
                Senha = txtSenha.Text, Perfil = cmbPerfil.SelectedItem.ToString(), Ativo = chkAtivo.Checked
            };
            if (idAtual == 0) dao.Inserir(u); else dao.Atualizar(u);
            return true;
        }

        protected override void Remover(int id)
        {
            if (id == Sessao.IdUsuario) throw new InvalidOperationException("Você não pode desativar o próprio usuário.");
            dao.Inativar(id);
        }
    }
}
