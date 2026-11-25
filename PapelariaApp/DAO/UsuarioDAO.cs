using System.Data;
using PapelariaApp.Data;
using PapelariaApp.Model;

namespace PapelariaApp.DAO
{
    public class UsuarioDAO
    {
        /// <summary>Retorna o usuário se login/senha estiverem corretos e ativo; senão null.</summary>
        public Usuario Autenticar(string login, string senha)
        {
            var dt = Db.Consultar(
                "SELECT id_usuario, nome, login, perfil FROM usuarios WHERE login=@l AND senha_hash=@s AND ativo=1",
                Db.P("@l", login), Db.P("@s", Seguranca.Sha256(senha)));
            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new Usuario
            {
                Id = Convert.ToInt32(r["id_usuario"]),
                Nome = r["nome"].ToString(),
                Login = r["login"].ToString(),
                Perfil = r["perfil"].ToString(),
                Ativo = true
            };
        }

        public DataTable Listar(string filtro) => Db.Consultar(
            @"SELECT id_usuario AS Id, nome AS Nome, login AS Login, perfil AS Perfil,
                     IF(ativo=1,'Sim','Não') AS Ativo
              FROM usuarios WHERE nome LIKE @f OR login LIKE @f ORDER BY nome",
            Db.P("@f", "%" + filtro + "%"));

        public void Inserir(Usuario u) => Db.Executar(
            "INSERT INTO usuarios (nome, login, senha_hash, perfil, ativo) VALUES (@n,@l,@s,@p,@a)",
            Db.P("@n", u.Nome), Db.P("@l", u.Login), Db.P("@s", Seguranca.Sha256(u.Senha)),
            Db.P("@p", u.Perfil), Db.P("@a", u.Ativo ? 1 : 0));

        /// <summary>Atualiza dados; a senha só é trocada se u.Senha vier preenchida.</summary>
        public void Atualizar(Usuario u)
        {
            if (string.IsNullOrEmpty(u.Senha))
                Db.Executar("UPDATE usuarios SET nome=@n, login=@l, perfil=@p, ativo=@a WHERE id_usuario=@id",
                    Db.P("@n", u.Nome), Db.P("@l", u.Login), Db.P("@p", u.Perfil),
                    Db.P("@a", u.Ativo ? 1 : 0), Db.P("@id", u.Id));
            else
                Db.Executar("UPDATE usuarios SET nome=@n, login=@l, senha_hash=@s, perfil=@p, ativo=@a WHERE id_usuario=@id",
                    Db.P("@n", u.Nome), Db.P("@l", u.Login), Db.P("@s", Seguranca.Sha256(u.Senha)),
                    Db.P("@p", u.Perfil), Db.P("@a", u.Ativo ? 1 : 0), Db.P("@id", u.Id));
        }

        /// <summary>Exclusão lógica: o usuário tem histórico de vendas e movimentações.</summary>
        public void Inativar(int id) =>
            Db.Executar("UPDATE usuarios SET ativo=0 WHERE id_usuario=@id", Db.P("@id", id));
    }
}
