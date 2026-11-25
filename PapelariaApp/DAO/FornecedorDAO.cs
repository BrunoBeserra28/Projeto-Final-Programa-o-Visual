using System.Data;
using PapelariaApp.Data;
using PapelariaApp.Model;

namespace PapelariaApp.DAO
{
    public class FornecedorDAO
    {
        public DataTable Listar(string filtro) => Db.Consultar(
            @"SELECT id_fornecedor AS Id, razao_social AS Razao, cnpj AS CNPJ, telefone AS Telefone,
                     email AS Email, endereco AS Endereco
              FROM fornecedores WHERE ativo=1 AND (razao_social LIKE @f OR cnpj LIKE @f) ORDER BY razao_social",
            Db.P("@f", "%" + filtro + "%"));

        public void Inserir(Fornecedor f) => Db.Executar(
            "INSERT INTO fornecedores (razao_social, cnpj, telefone, email, endereco) VALUES (@r,@c,@t,@e,@end)",
            Db.P("@r", f.RazaoSocial), Db.P("@c", f.Cnpj), Db.P("@t", f.Telefone), Db.P("@e", f.Email), Db.P("@end", f.Endereco));

        public void Atualizar(Fornecedor f) => Db.Executar(
            "UPDATE fornecedores SET razao_social=@r, cnpj=@c, telefone=@t, email=@e, endereco=@end WHERE id_fornecedor=@id",
            Db.P("@r", f.RazaoSocial), Db.P("@c", f.Cnpj), Db.P("@t", f.Telefone), Db.P("@e", f.Email),
            Db.P("@end", f.Endereco), Db.P("@id", f.Id));

        /// <summary>Exclusão lógica (o fornecedor pode ter pedidos de compra).</summary>
        public void Inativar(int id) =>
            Db.Executar("UPDATE fornecedores SET ativo=0 WHERE id_fornecedor=@id", Db.P("@id", id));
    }
}
