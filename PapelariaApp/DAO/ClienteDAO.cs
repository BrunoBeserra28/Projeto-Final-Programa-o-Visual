using System.Data;
using PapelariaApp.Data;
using PapelariaApp.Model;

namespace PapelariaApp.DAO
{
    public class ClienteDAO
    {
        public DataTable Listar(string filtro) => Db.Consultar(
            @"SELECT id_cliente AS Id, nome AS Nome, cpf AS CPF, telefone AS Telefone, email AS Email,
                     endereco AS Endereco, cidade AS Cidade, uf AS UF
              FROM clientes WHERE nome LIKE @f OR cpf LIKE @f ORDER BY nome",
            Db.P("@f", "%" + filtro + "%"));

        /// <summary>Lista para ComboBox, com a opção "Consumidor final" (Id = 0).</summary>
        public DataTable ListarCombo()
        {
            var dt = Db.Consultar("SELECT id_cliente AS Id, nome AS Nome FROM clientes ORDER BY nome");
            var linha = dt.NewRow();
            linha["Id"] = 0;
            linha["Nome"] = "(Consumidor final)";
            dt.Rows.InsertAt(linha, 0);
            return dt;
        }

        public void Inserir(Cliente c) => Db.Executar(
            @"INSERT INTO clientes (nome, cpf, telefone, email, endereco, cidade, uf)
              VALUES (@n,@cpf,@t,@e,@end,@c,@uf)",
            Db.P("@n", c.Nome), Db.P("@cpf", c.Cpf), Db.P("@t", c.Telefone), Db.P("@e", c.Email),
            Db.P("@end", c.Endereco), Db.P("@c", c.Cidade), Db.P("@uf", c.Uf));

        public void Atualizar(Cliente c) => Db.Executar(
            @"UPDATE clientes SET nome=@n, cpf=@cpf, telefone=@t, email=@e, endereco=@end, cidade=@c, uf=@uf
              WHERE id_cliente=@id",
            Db.P("@n", c.Nome), Db.P("@cpf", c.Cpf), Db.P("@t", c.Telefone), Db.P("@e", c.Email),
            Db.P("@end", c.Endereco), Db.P("@c", c.Cidade), Db.P("@uf", c.Uf), Db.P("@id", c.Id));

        public void Excluir(int id) =>
            Db.Executar("DELETE FROM clientes WHERE id_cliente=@id", Db.P("@id", id));
    }
}
