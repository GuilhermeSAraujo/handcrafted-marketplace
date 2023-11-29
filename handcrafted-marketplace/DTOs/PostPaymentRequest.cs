namespace handcrafted_marketplace.DTOs
{
    public class PostPaymentRequest
    {
        public string CpfUsuario { get; set; }
        public string Nome { get; set; }
        public int IdProduto { get; set; }
        public Pagamento DadosPagamento { get; set; }

        public bool IsValid
        {
            get => !(string.IsNullOrEmpty(CpfUsuario) || CpfUsuario.Length != 11 ||  IdProduto <= 0 || string.IsNullOrEmpty(Nome));
        }

        public class Pagamento
        {
            public string ContaCorrente { get; set; }
            public string Agencia { get; set; }
        }
    }
}
