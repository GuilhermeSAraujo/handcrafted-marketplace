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
    }
}
