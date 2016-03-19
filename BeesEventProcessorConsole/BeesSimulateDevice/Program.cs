using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading;

namespace BeesSimulateDevice
{
    class Program
    {
        static DeviceClient deviceClient;
        static string iotHubUri = "BeesIoTHub.azure-devices.net";
        static string deviceName = "fakeMessageSender";
        static string deviceKey = "nVldFZTuivT0/EeswEvrdB4oixJ6D3VEtBzP3dv2krg=";

        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device\n");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceName, deviceKey));

            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            double avgWindSpeed = 10; // m/s
            Random rand = new Random();

            //while (true)
            {
                double currentWindSpeed = avgWindSpeed + rand.NextDouble() * 4 - 2;

                var telemetryDataPoint = new
                {
                    deviceId = deviceName,
                    id = "100",
                    data = "01020304",
                    from = "12345",
                    lat = "123",
                    lng = "45"
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.MessageId = Guid.NewGuid().ToString();

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                Thread.Sleep(1000);
            }
        }
    }
}
