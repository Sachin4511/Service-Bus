// See https://aka.ms/new-console-template for more information
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Security.KeyVault.Secrets;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.Xml;
using Topic_Subscriptios;

   

    
Console.WriteLine("Topic And Subscription");
//Json config
/*IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appSettings.json");
IConfiguration config = builder.Build();
string connectionstring = null;
string appconfig = null;
if (config.GetConnectionString("ServiceBusString") != null && config.GetConnectionString("ApplicationInsights") != null)
{
    connectionstring = config.GetConnectionString("ServiceBusString");
    appconfig = config.GetConnectionString("ApplicationInsights");
}*/
String uriValue = "https://sakeyvault123.vault.azure.net/";
var azureKeyVault = new SecretClient(new Uri(uriValue),new DefaultAzureCredential());
var connectionstring = azureKeyVault.GetSecret("sapolicy123"); //sql server value is inserted as secret value
var appconfig = azureKeyVault.GetSecret("applicationinsight"); //application insight access key value as secret value

string topicName = "saservicebustopiic";
string Subscription = "Saservicebus12";
/*string connectionstring = "Endpoint=sb://servicebusdemoexample12345.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=d5DrvZ8FB5M0G3EV+fgEbtxmoNoXWMcLN+ASbIIylz8=";
    string topicName = "mytopic";
    string Subscription = "S1";*/

//logging
IServiceCollection services = new ServiceCollection();   
var channel = new ServerTelemetryChannel();
services.Configure<TelemetryConfiguration>(
    (config) =>
    {
        config.TelemetryChannel = channel;
    }
    );
services.AddLogging(builder =>
{
    builder.AddApplicationInsights(appconfig.Value.Value);
   
});

IServiceProvider serviceProvider = services.BuildServiceProvider();
ILogger<Program> logger = serviceProvider.GetRequiredService<ILogger<Program>>();


List<OrderDB> orders = new List<OrderDB>()
      {
               new OrderDB(){orderId=01,Quantity=100,UnitPrice=9.99f},
               new OrderDB(){orderId=02,Quantity=190,UnitPrice=10.99f},
               new OrderDB(){orderId=03,Quantity=200,UnitPrice=8.99f},


      };

    await sendMessage(orders);


    async Task sendMessage(List<OrderDB> orders)
    {
        ServiceBusClient serviceBusClient = new ServiceBusClient(connectionstring.Value.Value);
        ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(topicName);
        try
        {
            ServiceBusMessageBatch serviceBusMessageBatch = await serviceBusSender.CreateMessageBatchAsync();

            int messageId = 0;
            foreach (OrderDB order in orders)
            {
                ServiceBusMessage serviceBusMessage = new ServiceBusMessage(JsonConvert.SerializeObject(order));
                serviceBusMessage.ContentType = "application/json";
                serviceBusMessage.MessageId = messageId.ToString();
                messageId++;
                if (!serviceBusMessageBatch.TryAddMessage(
                     serviceBusMessage))
                {
                    throw new Exception("Error occured");
                }

            }


            Console.WriteLine("Sending Messages:" + messageId);
            await serviceBusSender.SendMessagesAsync(serviceBusMessageBatch);
            await serviceBusSender.DisposeAsync();
            await serviceBusClient.DisposeAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        await ReceiveMessages();


        async Task ReceiveMessages()
        {
            ServiceBusClient serviceBusClient1 = new ServiceBusClient(connectionstring.Value.Value);
            ServiceBusReceiver serviceBusReceiver1 = serviceBusClient1.CreateReceiver(topicName, Subscription,
                new ServiceBusReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete });
            IAsyncEnumerable<ServiceBusReceivedMessage> messages = serviceBusReceiver1.ReceiveMessagesAsync();
            await foreach (ServiceBusReceivedMessage message in messages)
            {
                try
                {
                    var result = JsonConvert.DeserializeObject<OrderDB>(message.Body.ToString());
                    logger.LogInformation("Receiving Message from subscription for OrderId:"+result.orderId);

                    OrderContext context = new OrderContext();

                    var orderObj = new OrderDB()
                    {
                        //orderId= result.orderId,
                        Quantity = result.Quantity,
                        UnitPrice = result.UnitPrice
                    };
                    context.Orders.Add(orderObj);
                    context.SaveChanges();
                logger.LogInformation("Data inserted:" + DateTime.Now);

                Console.WriteLine("Data inserted successfully");
                }
                catch (Exception e)
                {
                        logger.LogError(e.Message);
                    }
                /*Console.WriteLine("Order Id {0}", order.orderId);
                Console.WriteLine("Quantity {0}", order.Quantity);
                Console.WriteLine("Unit Price {0}", order.UnitPrice);*/
            }

        }


    }

