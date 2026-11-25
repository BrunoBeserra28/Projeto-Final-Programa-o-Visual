using System.Data;
using MySqlConnector;

namespace PapelariaApp.Data
{
    /// <summary>
    /// Auxiliar de acesso a dados. TODAS as consultas usam parâmetros (@nome),
    /// nunca concatenação de texto, evitando SQL Injection.
    /// </summary>
    public static class Db
    {
        public static MySqlParameter P(string nome, object valor) =>
            new MySqlParameter(nome, valor ?? DBNull.Value);

        public static DataTable Consultar(string sql, params MySqlParameter[] ps)
        {
            using var con = Conexao.Criar();
            con.Open();
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddRange(ps);
            using var rd = cmd.ExecuteReader();
            var dt = new DataTable();
            dt.Load(rd);
            return dt;
        }

        public static int Executar(string sql, params MySqlParameter[] ps)
        {
            using var con = Conexao.Criar();
            con.Open();
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddRange(ps);
            return cmd.ExecuteNonQuery();
        }

        public static object Escalar(string sql, params MySqlParameter[] ps)
        {
            using var con = Conexao.Criar();
            con.Open();
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddRange(ps);
            return cmd.ExecuteScalar();
        }

        /// <summary>Traduz erros comuns do MySQL em mensagens amigáveis.</summary>
        public static string Mensagem(Exception ex)
        {
            if (ex is MySqlException my)
            {
                switch (my.Number)
                {
                    case 1062: return "Já existe um registro com esse valor único (CPF, CNPJ, login, código de barras ou nome).";
                    case 1451: return "Este registro está em uso em outras tabelas e não pode ser excluído.";
                    case 1452: return "Referência inválida: o registro relacionado não existe.";
                    case 3819: return "Algum valor informado viola uma restrição do banco (CHECK).";
                    case 0:
                    case 1042:
                    case 1045:
                        return "Não foi possível conectar ao MySQL. Verifique a string de conexão em Data/Conexao.cs e se o servidor está ativo.\n\nDetalhe: " + my.Message;
                }
                return "Erro de banco de dados: " + my.Message;
            }
            return ex.Message;
        }
    }
}
