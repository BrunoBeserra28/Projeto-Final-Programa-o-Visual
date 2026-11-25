namespace PapelariaApp.Model
{
    public class Produto
    {
        public int Id { get; set; }
        public string CodigoBarras { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal PrecoCusto { get; set; }
        public decimal PrecoVenda { get; set; }
        public int IdCategoria { get; set; }
        public int Quantidade { get; set; }      // saldo em estoque
        public int EstoqueMinimo { get; set; }
    }
}
