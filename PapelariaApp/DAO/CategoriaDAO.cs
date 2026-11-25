using System.Data;
using PapelariaApp.Data;
using PapelariaApp.Model;

namespace PapelariaApp.DAO
{
    public class CategoriaDAO
    {
        public DataTable Listar(string filtro) => Db.Consultar(
            "SELECT id_categoria AS Id, nome AS Nome, descricao AS Descricao FROM categorias WHERE nome LIKE @f ORDER BY nome",
            Db.P("@f", "%" + filtro + "%"));

        public DataTable ListarCombo() =>
            Db.Consultar("SELECT id_categoria AS Id, nome AS Nome FROM categorias ORDER BY nome");

        public void Inserir(Categoria c) => Db.Executar(
            "INSERT INTO categorias (nome, descricao) VALUES (@n,@d)", Db.P("@n", c.Nome), Db.P("@d", c.Descricao));

        public void Atualizar(Categoria c) => Db.Executar(
            "UPDATE categorias SET nome=@n, descricao=@d WHERE id_categoria=@id",
            Db.P("@n", c.Nome), Db.P("@d", c.Descricao), Db.P("@id", c.Id));

        public void Excluir(int id) =>
            Db.Executar("DELETE FROM categorias WHERE id_categoria=@id", Db.P("@id", id));
    }
}
