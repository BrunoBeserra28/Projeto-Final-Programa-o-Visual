using PapelariaApp.Model;

namespace PapelariaApp.Data
{
    /// <summary>Guarda o usuário logado durante a execução.</summary>
    public static class Sessao
    {
        public static Usuario Atual { get; set; }
        public static int IdUsuario => Atual?.Id ?? 0;
    }
}
