using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PapelariaApp.Data
{
    /// <summary>Hash de senha e validações simples de campos.</summary>
    public static class Seguranca
    {
        /// <summary>SHA-256 em hexadecimal minúsculo (igual ao SHA2(x,256) do MySQL).</summary>
        public static string Sha256(string texto)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto ?? ""));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }

    public static class Validador
    {
        public static bool QtdDigitos(string texto, int qtd) =>
            Regex.Replace(texto ?? "", @"\D", "").Length == qtd;

        public static bool EmailValido(string email) =>
            Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
