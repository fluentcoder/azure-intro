using Microsoft.Azure.ServiceBus;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication.Controllers
{
    public static class ServiceBusProcessor
    {
        public static string logQueueConnectionString { get; set; }
        public static QueueClient queueClient { get; set; }

        public static async Task RegisterOnMessageHandlerAndReceiveMessages(QueueClient client, string logConnectionString)
        {
            logQueueConnectionString = logConnectionString;
            queueClient = client;
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
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
            await Log("Message handler registered");
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {

            await Log("Message handler started message processing");

            string messageContent =
                $"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}";

            await queueClient.CompleteAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
            await Log("Message processed :" + messageContent);
        }

        static async Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            await Log(exceptionReceivedEventArgs.Exception.ToString());
        }

        static async Task Log(string messageContent)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(logQueueConnectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("debuglog");
            CloudQueueMessage message = new CloudQueueMessage(messageContent);
            await queue.AddMessageAsync(message);
        }
    }
}
