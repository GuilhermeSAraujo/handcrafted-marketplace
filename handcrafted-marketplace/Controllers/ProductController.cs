using Azure.Core;
using handcrafted_marketplace.DTOs;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace handcrafted_marketplace.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly NpgsqlConnection _conn;

        public ProductController(NpgsqlConnection conn)
        {
            _conn = conn ?? throw new ArgumentNullException(nameof(conn));
        }

        [HttpPost]
        public async Task<IActionResult> PostProduct([FromBody] PostProductRequest request)
        {
            if (!request.IsValid) return UnprocessableEntity(request);

            await using (_conn)
            {
                await _conn.OpenAsync();

                await using var cmd = new NpgsqlCommand("INSERT INTO produto (nome, preco, cnpjloja) VALUES ($1, $2, $3)", _conn)
                {
                    Parameters =
                        {
                            new NpgsqlParameter { Value = request.Nome },
                            new NpgsqlParameter { Value = request.Preco },
                            new NpgsqlParameter { Value = request.CnpjLoja }
                        }
                };
                await cmd.ExecuteNonQueryAsync();
            }
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult>  GetProducts()
        {
            var products = new List<GetProductsResponse>();
            await using (_conn)
            {
                await _conn.OpenAsync();

                await using var cmd = new NpgsqlCommand("SELECT p.id, p.nome, p.preco, l.cnpj, l.nome FROM produto p  INNER JOIN loja l ON p.cnpjloja = l.cnpj", _conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while(await reader.ReadAsync())
                {
                    var productResponse = new GetProductsResponse
                    {
                        Product = new GetProductsResponse.ProductDetails
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Price = reader.GetDouble(2),
                        },
                        Store = new GetProductsResponse.StoreDetails
                        {
                            Cnpj = reader.GetString(3),
                            Name = reader.GetString(4),
                        }
                    };
                    products.Add(productResponse);
                }
            }
            return Ok(products);
        }
    }
}
