namespace handcrafted_marketplace.DTOs
{
    public class GetPaymentsResponse
    {
        public DadosPagamento Pagamento { get; set; }
        public DadosUsuario Usuario { get; set; }
        public DadosProduto Produto { get; set; }
        public DadosLoja Loja { get; set; }

        public class DadosPagamento
        {
            public int Id { get; set; }
            public string ContaCorrente { get; set; }
            public string Agencia { get; set; }
            public string Status { get; set; }
        }

        public class DadosUsuario
        {
            public string Nome { get; set; }
            public string Cpf { get; set; }
        }

        public class DadosProduto
        {
            public string Nome { get; set; }
            public double Preco { get; set; }
        }

        public class DadosLoja
        {
            public string Nome { get; set; }
            public string Cnpj { get; set; }
        }
    }
}
