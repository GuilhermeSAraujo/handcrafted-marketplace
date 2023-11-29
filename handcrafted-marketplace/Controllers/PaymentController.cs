using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using handcrafted_marketplace.DTOs;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;

namespace handcrafted_marketplace.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly NpgsqlConnection _conn;

        public PaymentController(NpgsqlConnection conn)
        {
            _conn = conn ?? throw new ArgumentNullException(nameof(conn));
        }

        [HttpPost]
        public async Task<IActionResult> PostPayment([FromBody] PostPaymentRequest request)
        {
            if (!request.IsValid) return UnprocessableEntity(request);

            int paymentId = 0;

            await using (_conn)
            {
                await _conn.OpenAsync();

                await using var cmdInsertOrUpdateUser = new NpgsqlCommand(@"
                    INSERT INTO usuario (nome, cpf) 
                    VALUES ($1, $2)
                    ON CONFLICT (cpf) DO NOTHING", _conn)
                {
                    Parameters =
                    {
                        new NpgsqlParameter { Value = request.Nome },
                        new NpgsqlParameter { Value = request.CpfUsuario }
                    }
                };

                await cmdInsertOrUpdateUser.ExecuteNonQueryAsync();

                await using var cmd = new NpgsqlCommand("INSERT INTO pagamento (idproduto, cpfusuario, contacorrente, agencia, status) VALUES ($1, $2, $3, $4, $5) RETURNING id", _conn)
                {
                    Parameters =
            {
                new NpgsqlParameter { Value = request.IdProduto },
                new NpgsqlParameter { Value = request.CpfUsuario },
                new NpgsqlParameter { Value = request.DadosPagamento.ContaCorrente },
                new NpgsqlParameter { Value = request.DadosPagamento.Agencia },
                new NpgsqlParameter { Value = "EM ANDAMENTO" },
            }
                };
                paymentId = (int)await cmd.ExecuteScalarAsync();
            }

            var connectionString = "";
            var eventHubName = "payment-service";

            await using (var producer = new EventHubProducerClient(connectionString, eventHubName))
            {
                using EventDataBatch eventBatch = await producer.CreateBatchAsync();
                eventBatch.TryAdd(new EventData(new BinaryData(new
                {
                    idPagamento = paymentId,
                    idProduto = request.IdProduto,
                    cpfUsuario = request.CpfUsuario,
                    contaCorrente = request.DadosPagamento.ContaCorrente,
                    agencia = request.DadosPagamento.Agencia
                })));

                await producer.SendAsync(eventBatch);
            }
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            var products = new List<GetPaymentsResponse>();
            await using (_conn)
            {
                await _conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(@"
	                  SELECT 
	                      pagamento.id, pagamento.contacorrente, pagamento.agencia, pagamento.status,
	                      u.nome as nomeUsuario, u.cpf,
	                      produto.nome as nomeProduto, produto.preco,
	                      loja.cnpj, loja.nome
	                  FROM pagamento
	                  INNER JOIN produto ON pagamento.idproduto = produto.id
	                  INNER JOIN usuario u ON u.cpf = pagamento.cpfusuario
	                  INNER JOIN loja ON loja.cnpj = produto.cnpjloja
                    ", _conn);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var paymentResponse = new GetPaymentsResponse
                    {
                        Pagamento = new GetPaymentsResponse.DadosPagamento
                        {
                            Id = reader.GetInt32(0),
                            ContaCorrente = reader.GetString(1),
                            Agencia = reader.GetString(2),
                            Status = reader.GetString(3)
                        },
                        Usuario = new GetPaymentsResponse.DadosUsuario
                        {
                            Nome = reader.GetString(4),
                            Cpf = reader.GetString(5),
                        },
                        Produto = new GetPaymentsResponse.DadosProduto
                        {
                            Nome = reader.GetString(6),
                            Preco = reader.GetDouble(7),
                        },
                        Loja = new GetPaymentsResponse.DadosLoja
                        {
                            Cnpj = reader.GetString(8),
                            Nome = reader.GetString(9)
                        }
                    };
                    products.Add(paymentResponse);
                }
            }
            return Ok(products);
        }
    }
}
