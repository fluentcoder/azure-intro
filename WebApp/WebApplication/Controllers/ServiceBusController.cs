using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
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
        public async Task<ActionResult> SendMessage([FromRoute] string queueName, [FromBody] string content)
        {
            try
            {
                _queueClient = new QueueClient(_serviceBusConnectionString, queueName);
                string messageBody = content;
                Message message = new Message(Encoding.UTF8.GetBytes(messageBody)) { SessionId = _queueClient.ClientId };

                // Send the message to the queue.
                await _queueClient.SendAsync(message);
                return Ok($"Message \"{messageBody}\" sended");
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }

        // GET: api/ServiceBus/queue/name
        [HttpGet("queue/{queueName}")]
        public async Task<ActionResult> ProcessMessage([FromRoute] string queueName)
        {
            _queueClient = new QueueClient(_serviceBusConnectionString, queueName);

            RegisterOnMessageHandlerAndReceiveMessages();

            return Ok("done");
        }

        static async Task RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false,
            };

            // Register the function that will process messages
            _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
            await Log("Message handler registered");
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {

            await Log("Message handler started message processing");

            string messageContent =
                $"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}";

            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
            await Log("Message processed :"+messageContent);
        }

        static async Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            await Log(exceptionReceivedEventArgs.Exception.ToString());

        }

        static async Task Log(string messageContent)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_logQueueConnectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("debuglog");
            CloudQueueMessage message = new CloudQueueMessage(messageContent);
            await queue.AddMessageAsync(message);
        }
    }
}
