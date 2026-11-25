namespace PapelariaApp.Model
{
    public class Venda
    {
        public int Id { get; set; }
        public DateTime Data { get; set; } = DateTime.Now;
        public int? IdCliente { get; set; }     // null = consumidor final
        public int IdUsuario { get; set; }
        public string FormaPagamento { get; set; } = "DINHEIRO";
        public List<ItemVenda> Itens { get; set; } = new List<ItemVenda>();

        /// <summary>Cálculo automático do valor total da venda.</summary>
        public decimal Total => Itens.Sum(i => i.Subtotal);
    }
}
