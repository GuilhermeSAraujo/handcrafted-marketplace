namespace handcrafted_marketplace.DTOs
{
    public class GetProductsResponse
    {
        public ProductDetails Product { get; set; }
        public StoreDetails Store { get; set; }

        public class ProductDetails
        {
            public string Name { get; set; }
            public double Price { get; set; }
        }

        public class StoreDetails
        {
            public string Cnpj { get; set; }
            public string Name { get; set; }
        }
    }
}
