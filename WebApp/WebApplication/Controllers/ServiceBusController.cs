using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceBusController : ControllerBase
    {
        private string _serviceBusConnectionString;
        private static string _logQueueConnectionString;
        private static QueueClient _queueClient;

        public ServiceBusController(IConfiguration iConfig)
        {
            _serviceBusConnectionString = iConfig.GetSection("ConnectionStrings").GetSection("ServiceBusConnectionString").Value;
            _logQueueConnectionString = iConfig.GetValue<string>("ConnectionStrings:StorageAccountConnectionString");
        }

        // POST: api/ServiceBus/queue/name
        [HttpPost("queue/{queueName}")]
        public async Task<ActionResult> SendMessages([FromRoute] string queueName, [FromBody] string[] content)
        {
            

            try
            {
                _queueClient = new QueueClient(_serviceBusConnectionString, queueName);
                string report = string.Empty;

                foreach (var message in content )
                {
                    string messageBody = message;
                    Message messageToSend = new Message(Encoding.UTF8.GetBytes(messageBody));

                    await _queueClient.SendAsync(messageToSend);

                    report+=$"Message \"{messageBody}\" sended"+Environment.NewLine;
                }
                return Ok(report + Environment.NewLine + "All messages was sended.");
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }


        // POST: api/ServiceBus/queue/register/name
        [HttpPost("queue/register/{queueName}")]
        public async Task<ActionResult> RegisterMessageHandler([FromRoute] string queueName)
        {
            await ServiceBusProcessor.RegisterOnMessageHandlerAndReceiveMessages(
                new QueueClient(_serviceBusConnectionString, queueName), _logQueueConnectionString);

            return Ok($"MessageHandler was registered on queue: \"{queueName}\"");
        }

        
    }
}
