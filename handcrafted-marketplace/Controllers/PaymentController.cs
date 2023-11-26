using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using handcrafted_marketplace.DTOs;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Text;

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
    }
}
