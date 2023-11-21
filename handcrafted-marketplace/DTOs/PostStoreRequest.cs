namespace handcrafted_marketplace.DTOs
{
    public class PostStoreRequest
    {
        public string Cnpj { get; set; }
        public string Nome { get; set; }
        public bool IsValid
        {
            get => !(string.IsNullOrEmpty(Cnpj) || string.IsNullOrEmpty(Nome));
        }
    }
}
