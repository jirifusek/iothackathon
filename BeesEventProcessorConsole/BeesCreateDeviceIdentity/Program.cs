using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using System.IO;

namespace BeesCreateDeviceIdentity
{
    class Program
    {
        static RegistryManager registryManager;
        static string connectionString = "HostName=BeesIoTHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=fW2cDMDMeQhSZxRGzRlPGm6vtRE8AYX2ljxdcNccHYo=";

        static string deviceFile = @"";

        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            AddDeviceAsync().Wait();
            Console.ReadLine();
        }

        private async static Task AddDeviceAsync()
        {
            string deviceId = "fakeMessageSender";
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
            using (var fw = new StreamWriter(deviceFile))
            {
                fw.WriteLine("{");
                fw.WriteLine("  \"UserName\" : \"{0}\"", deviceId);
                fw.WriteLine("  \"PrimaryKey\" : \"{0}\"", device.Authentication.SymmetricKey.PrimaryKey);
                fw.WriteLine("}");
            }
        }
    }
}
