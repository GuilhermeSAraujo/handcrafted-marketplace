namespace handcrafted_marketplace.DTOs
{
    public class PostUserRequest
    {
        public string Name { get; set; }
        public string Cpf { get; set; }

        public bool IsValid
        {
            get => !(string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Cpf));
        }
    }
}
