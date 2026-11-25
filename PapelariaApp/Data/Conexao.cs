using MySqlConnector;

namespace PapelariaApp.Data
{
    /// <summary>
    /// Classe de conexão com o MySQL.
    /// Ajuste a string de conexão (usuário/senha) conforme o seu ambiente.
    /// Também é possível definir a variável de ambiente PAPELARIA_CONN.
    /// </summary>
    public static class Conexao
    {
        private const string Padrao =
            "Server=localhost;Port=3306;Database=papelaria;Uid=root;Pwd=SUA_SENHA;";

        public static string ConnectionString =>
            Environment.GetEnvironmentVariable("PAPELARIA_CONN") ?? Padrao;

        public static MySqlConnection Criar() => new MySqlConnection(ConnectionString);
    }
}
