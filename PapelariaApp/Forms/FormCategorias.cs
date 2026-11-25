using System.Data;
using System.Windows.Forms;
using PapelariaApp.DAO;
using PapelariaApp.Model;

namespace PapelariaApp.Forms
{
    public class FormCategorias : FormCadastroBase
    {
        private readonly CategoriaDAO dao = new CategoriaDAO();
        private TextBox txtNome, txtDesc;

        public FormCategorias() : base("Categorias", 50)
        {
            pnlCampos.Controls.AddRange(new Control[]
            {
                UI.Lbl("Nome*", 0, 5),      txtNome = UI.Txt(70, 5, 250),
                UI.Lbl("Descrição", 350, 5), txtDesc = UI.Txt(430, 5, 400)
            });
        }

        protected override DataTable Consultar(string filtro) => dao.Listar(filtro);

        protected override void PreencherCampos(DataGridViewRow r)
        {
            txtNome.Text = UI.S(r.Cells["Nome"].Value);
            txtDesc.Text = UI.S(r.Cells["Descricao"].Value);
        }

        protected override void LimparCampos() { txtNome.Clear(); txtDesc.Clear(); txtNome.Focus(); }

        protected override bool Salvar()
        {
            if (!UI.Obrigatorios((txtNome, "Nome"))) return false;
            var c = new Categoria { Id = idAtual, Nome = txtNome.Text.Trim(), Descricao = txtDesc.Text.Trim() };
            if (idAtual == 0) dao.Inserir(c); else dao.Atualizar(c);
            return true;
        }

        protected override void Remover(int id) => dao.Excluir(id);
    }
}
