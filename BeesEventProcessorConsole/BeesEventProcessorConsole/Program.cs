using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace BeesEventProcessorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string iotHubConnectionString = "HostName=BeesIoTHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=fW2cDMDMeQhSZxRGzRlPGm6vtRE8AYX2ljxdcNccHYo=";
            string iotHubD2cEndpoint = "messages/events";
            StoreEventProcessor.StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=beesiotstorage;AccountKey=Wh9T10yXUJKU7qWmY1Pu8Uiftn5EbDHN279hCH+Sj5CLG6SM2yv5jS0qtsDcgw3raOxURAGwANYp+N9J23XvNA==;BlobEndpoint=https://beesiotstorage.blob.core.windows.net/;TableEndpoint=https://beesiotstorage.table.core.windows.net/;QueueEndpoint=https://beesiotstorage.queue.core.windows.net/;FileEndpoint=https://beesiotstorage.file.core.windows.net/";
            StoreEventProcessor.ServiceBusConnectionString = "Endpoint=sb://beesservicebus.servicebus.windows.net/;SharedAccessKeyName=send;SharedAccessKey=Ukl02m/VX093E232Tr6EhWT+j57vSy1BltqJg3g0zqQ="; 

            string eventProcessorHostName = Guid.NewGuid().ToString();
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, iotHubD2cEndpoint, EventHubConsumerGroup.DefaultGroupName, iotHubConnectionString, StoreEventProcessor.StorageConnectionString, "messages-events");
            Console.WriteLine("Registering EventProcessor...");
            eventProcessorHost.RegisterEventProcessorAsync<StoreEventProcessor>().Wait();

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }
    }
}
