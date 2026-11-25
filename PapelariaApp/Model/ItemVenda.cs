namespace PapelariaApp.Model
{
    /// <summary>Item de uma venda (relação N:N Venda x Produto).</summary>
    public class ItemVenda
    {
        public int IdProduto { get; set; }
        public string NomeProduto { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Desconto { get; set; }   // desconto TOTAL da linha (R$)

        /// <summary>Cálculo automático do subtotal da linha.</summary>
        public decimal Subtotal => Quantidade * PrecoUnitario - Desconto;
    }
}
