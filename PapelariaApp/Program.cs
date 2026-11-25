using PapelariaApp.Data;
using PapelariaApp.Forms;

namespace PapelariaApp
{
    internal static class Program
    {
        /// <summary>Ponto de entrada: exibe o login e, se autenticado, abre o menu principal.</summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            while (true)
            {
                Sessao.Atual = null;
                using (var login = new FormLogin())
                {
                    if (login.ShowDialog() != DialogResult.OK) break; // usuário fechou o login
                }

                using (var principal = new FormPrincipal())
                {
                    Application.Run(principal);
                    if (!principal.Deslogou) break; // fechou a janela = encerra; "Sair" = volta ao login
                }
            }
        }
    }
}
