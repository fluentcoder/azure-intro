using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;
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
        private static TopicClient _topicClient;

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

                foreach (var message in content)
                {
                    string messageBody = message;
                    Message messageToSend = new Message(Encoding.UTF8.GetBytes(messageBody));

                    await _queueClient.SendAsync(messageToSend);

                    report += $"Message \"{messageBody}\" sended" + Environment.NewLine;
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


        // POST: api/ServiceBus/topic/register/name/name
        [HttpPost("topic/register/{topicName}/{subscriptionName}")]
        public async Task<ActionResult> RegisterMessageHandlerForTopic([FromRoute] string topicName, [FromRoute] string subscriptionName)
        {
            await ServiceBusProcessor.RegisterOnMessageHandlerAndReceiveMessagesFromTopic(
                new SubscriptionClient(_serviceBusConnectionString, topicName, subscriptionName), _logQueueConnectionString);

            return Ok($"MessageHandler was registered on topic: \"{topicName}\"");

        }

        // POST: api/ServiceBus/topic/name
        [HttpPost("topic/{topicName}")]
        public async Task<ActionResult> SendMessagesToTopic([FromRoute] string topicName, [FromBody] string[] content)
        {
            try
            {
                _topicClient = new TopicClient(_serviceBusConnectionString, topicName);

                string report = string.Empty;

                foreach (var message in content)
                {
                    string messageBody = message;
                    Message messageToSend = new Message(Encoding.UTF8.GetBytes(messageBody));

                    await _topicClient.SendAsync(messageToSend);

                    report += $"Message \"{messageBody}\" sended" + Environment.NewLine;
                }
                return Ok(report + Environment.NewLine + "All messages was sended.");
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }
    }
}
