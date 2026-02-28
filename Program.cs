using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Configuration;

class Program
{
    static async Task Main(string[] args)
    {
        string connectionString = AppConfiguration.ConnectionString;

        string queueName = AppConfiguration.QueueName;

        const int RETRY_MESSAGE_QUANTITY = 3;

        ServiceBusClient client = new ServiceBusClient(connectionString);

        ServiceBusSender sender = client.CreateSender(queueName);

        try
        {
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            for (int i = 1; i <= RETRY_MESSAGE_QUANTITY; i++)
            {
                if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
                {
                    throw new Exception($"The message {i} is too large to fit in the batch.");
                }
            }
            await sender.SendMessagesAsync(messageBatch);
            Console.WriteLine($"It had been published a batch of 3 messages in the queue.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }

        Console.WriteLine("They have been sent messages to the queue. Check the Azure portal to verify.");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
