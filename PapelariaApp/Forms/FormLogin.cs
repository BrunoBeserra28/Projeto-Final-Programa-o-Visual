using System.Drawing;
using System.Windows.Forms;
using PapelariaApp.Data;
using PapelariaApp.DAO;

namespace PapelariaApp.Forms
{
    public class FormLogin : Form
    {
        private readonly UsuarioDAO dao = new UsuarioDAO();
        private TextBox txtLogin, txtSenha;

        public FormLogin()
        {
            Text = "Papelaria - Login";
            ClientSize = new Size(360, 230);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            UI.Tema(this);

            var titulo = new Label
            {
                Text = "Sistema da Papelaria", Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = UI.Primaria, Left = 20, Top = 20, AutoSize = true
            };
            txtLogin = UI.Txt(100, 80, 220);
            txtSenha = UI.Txt(100, 115, 220);
            txtSenha.UseSystemPasswordChar = true;
            txtSenha.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { Entrar(); e.SuppressKeyPress = true; } };

            Controls.AddRange(new Control[]
            {
                titulo,
                UI.Lbl("Usuário", 20, 80), txtLogin,
                UI.Lbl("Senha", 20, 115),  txtSenha,
                UI.Btn("Entrar", 100, 165, (s, e) => Entrar(), 100),
                UI.Btn("Cancelar", 220, 165, (s, e) => DialogResult = DialogResult.Cancel, 100)
            });
        }

        private void Entrar()
        {
            if (!UI.Obrigatorios((txtLogin, "Usuário"), (txtSenha, "Senha"))) return;
            try
            {
                var u = dao.Autenticar(txtLogin.Text.Trim(), txtSenha.Text);
                if (u == null)
                {
                    UI.Aviso("Usuário ou senha inválidos.");
                    txtSenha.Clear(); txtSenha.Focus();
                    return;
                }
                Sessao.Atual = u;
                AuditoriaDAO.Registrar("LOGIN", "Login realizado: " + u.Login);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { UI.Erro(Db.Mensagem(ex)); }
        }
    }
}
