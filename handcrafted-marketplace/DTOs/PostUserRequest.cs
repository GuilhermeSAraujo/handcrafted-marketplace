namespace handcrafted_marketplace.DTOs
{
    public class PostUserRequest
    {
        public string Nome { get; set; }
        public string Cpf { get; set; }

        public bool IsValid
        {
            get => !(string.IsNullOrEmpty(Nome) || string.IsNullOrEmpty(Cpf));
        }
    }
}
