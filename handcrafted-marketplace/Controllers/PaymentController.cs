using Azure.Messaging.EventHubs.Producer;
using Confluent.Kafka;
using handcrafted_marketplace.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
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

            await using (_conn)
            {
                await _conn.OpenAsync();

                await using var cmd = new NpgsqlCommand("INSERT INTO pagamento (idproduto, cpfusuario, contacorrente, agencia, status) VALUES ($1, $2, $3, $4, $5)", _conn)
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
                await cmd.ExecuteNonQueryAsync();
            }

            EventHubProducerClient producerClient = new EventHubProducerClient(
                "",
                "payment-service");

            // Create a batch of events 
            using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

            for (int i = 1; i <= numOfEvents; i++)
            {
                if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes($"Event {i}"))))
                {
                    // if it is too large for the batch
                    throw new Exception($"Event {i} is too large for the batch and cannot be sent.");
                }
            }

            try
            {
                // Use the producer client to send the batch of events to the event hub
                await producerClient.SendAsync(eventBatch);
                Console.WriteLine($"A batch of {numOfEvents} events has been published.");
                Console.ReadLine();
            }
            finally
            {
                await producerClient.DisposeAsync();
            }

            return Ok();
        }
    }
}
