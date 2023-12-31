﻿using handcrafted_marketplace.DTOs;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace handcrafted_marketplace.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly NpgsqlConnection _conn;

        public UserController(NpgsqlConnection conn)
        {
            _conn = conn ?? throw new ArgumentNullException(nameof(conn));
        }

        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] PostUserRequest request)
        {
            if (!request.IsValid) return UnprocessableEntity(request);

            await using (_conn)
            {
                await _conn.OpenAsync();

                await using var cmd = new NpgsqlCommand("INSERT INTO usuario (nome, cpf) VALUES ($1, $2)", _conn)
                {
                    Parameters =
                        {
                            new NpgsqlParameter { Value = request.Nome },
                            new NpgsqlParameter { Value = request.Cpf }
                        }
                };
                await cmd.ExecuteNonQueryAsync();
            }

            return Ok();
        }
    }
}
