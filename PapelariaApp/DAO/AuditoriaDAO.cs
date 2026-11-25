using PapelariaApp.Data;

namespace PapelariaApp.DAO
{
    /// <summary>Registro de operações (auditoria). Nunca deve derrubar a operação principal.</summary>
    public class AuditoriaDAO
    {
        public static void Registrar(string acao, string detalhe)
        {
            try
            {
                Db.Executar("INSERT INTO auditoria (id_usuario, acao, detalhe) VALUES (@u,@a,@d)",
                    Db.P("@u", Sessao.Atual == null ? null : (object)Sessao.Atual.Id),
                    Db.P("@a", acao), Db.P("@d", detalhe));
            }
            catch { /* auditoria é complementar: falhas aqui são ignoradas */ }
        }
    }
}
