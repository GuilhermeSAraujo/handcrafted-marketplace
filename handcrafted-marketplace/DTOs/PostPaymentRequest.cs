namespace handcrafted_marketplace.DTOs
{
    public class PostPaymentRequest
    {
        public string CpfUsuario { get; set; }
        public int IdProduto { get; set; }
        public Pagamento DadosPagamento { get; set; }

        public bool IsValid
        {
            get => !(string.IsNullOrEmpty(CpfUsuario) || IdProduto <= 0);
        }

        public class Pagamento
        {
            public string ContaCorrente { get; set; }
            public string Agencia { get; set; }
        }
    }
}
