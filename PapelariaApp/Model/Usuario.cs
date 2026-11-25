namespace PapelariaApp.Model
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Login { get; set; }
        public string Senha { get; set; }   // texto puro apenas em memória; no banco vai o hash
        public string Perfil { get; set; }  // ADMIN | VENDEDOR | ESTOQUISTA
        public bool Ativo { get; set; } = true;
    }
}
