using handcrafted_marketplace.DTOs;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace handcrafted_marketplace.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoreController : ControllerBase
    {
        private readonly NpgsqlConnection _conn;

        public StoreController(NpgsqlConnection conn)
        {
            _conn = conn ?? throw new ArgumentNullException(nameof(conn));
        }

        [HttpPost]
        public async Task<IActionResult> PostStore([FromBody] PostStoreRequest request)
        {
            if (!request.IsValid) return UnprocessableEntity(request);

            await using (_conn)
            {
                await _conn.OpenAsync();

                await using var cmd = new NpgsqlCommand("INSERT INTO loja (cnpj, nome) VALUES ($1, $2)", _conn)
                {
                    Parameters =
                        {
                            new NpgsqlParameter { Value = request.Cnpj },
                            new NpgsqlParameter { Value = request.Nome }
                        }
                };
                await cmd.ExecuteNonQueryAsync();
            }
            return Ok();
        }
    }
}
