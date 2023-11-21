namespace handcrafted_marketplace.DTOs
{
    public class PostProductRequest
    {
        public string Nome { get; set; }
        public float Preco { get; set; }
        public string CnpjLoja { get; set; }

        public bool IsValid {
            get => !(string.IsNullOrEmpty(Nome) || Preco <= 0 || string.IsNullOrEmpty(CnpjLoja));
        }
    }
}
