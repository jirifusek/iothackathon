using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace BeesEventProcessorConsole
{
    class StoreEventProcessor : IEventProcessor
    {
        private const int MAX_BLOCK_SIZE = 4 * 1024 * 1024;
        public static string StorageConnectionString;
        public static string ServiceBusConnectionString;

        public static string dbcn = "Server=tcp:beesportal.database.windows.net,1433;Database=beesportal;User ID=beesportal@beesportal;Password=hackathon1!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        private CloudBlobClient blobClient;
        private CloudBlobContainer blobContainer;
        private QueueClient queueClient;

        private long currentBlockInitOffset;
        private MemoryStream toAppend = new MemoryStream(MAX_BLOCK_SIZE);

        private Stopwatch stopwatch;
        private TimeSpan MAX_CHECKPOINT_TIME = TimeSpan.FromHours(1);

        public StoreEventProcessor()
        {
            var storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            blobClient = storageAccount.CreateCloudBlobClient();
            blobContainer = blobClient.GetContainerReference("beesblobs");
            blobContainer.CreateIfNotExists();
            queueClient = QueueClient.CreateFromConnectionString(ServiceBusConnectionString, "beesalarmqueue");
        }

        Task IEventProcessor.CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine("Processor Shutting Down. Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason);
            return Task.FromResult<object>(null);
        }

        Task IEventProcessor.OpenAsync(PartitionContext context)
        {
            Console.WriteLine("StoreEventProcessor initialized.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset);

            if (!long.TryParse(context.Lease.Offset, out currentBlockInitOffset))
            {
                currentBlockInitOffset = 0;
            }
            stopwatch = new Stopwatch();
            stopwatch.Start();

            return Task.FromResult<object>(null);
        }

        async Task IEventProcessor.ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            var cnt = 0;
            foreach (EventData eventData in messages)
            {
                cnt++;
                //if (cnt < 4) // in time requires adjusment !!!!!!!!!!! skip X test messsages !!!!!!!!
                //    continue;

                //if (eventData.Properties.ContainsKey("messageType") && (string)eventData.Properties["messageType"] == "interactive")
                //    continue;
                //if (eventData.SystemProperties.Keys.Contains("message-id"))
                //    continue;

                if (eventData.SystemProperties.Count < 11)
                    continue;

                byte[] data = eventData.GetBytes();

                //var messageId = (string)eventData.SystemProperties["message-id"];

                var jsonnew = DeserializeMessage(data);

                if (jsonnew.MessageType == "alarm")
                {
                    var datanew = JsonConvert.SerializeObject(jsonnew);
                    data = Encoding.UTF8.GetBytes(datanew);

                    var queueMessage = new BrokeredMessage(new MemoryStream(data));
                    queueMessage.MessageId = Guid.NewGuid().ToString();
                    await queueClient.SendAsync(queueMessage);

                    using (var conn = new SqlConnection(dbcn))
                    {
                        await conn.OpenAsync();

                        using (var command = new SqlCommand("INSERT INTO Notifications(sigfoxId, severity, text, Datetime, Viewed) values(@sigfoxId, @severity, @text, @Datetime, @Viewed)"))
                        {
                            command.Connection = conn;
                            command.Parameters.AddWithValue("@sigfoxId", jsonnew.Device);
                            command.Parameters.AddWithValue("@severity", "alarm");
                            command.Parameters.AddWithValue("@text", "GSM");
                            command.Parameters.AddWithValue("@Datetime", DateTime.Now);
                            command.Parameters.AddWithValue("@Viewed", false);

                            command.ExecuteNonQuery();
                        }
                    }

                    WriteHighlightedMessage(string.Format("Received interactive message: {0}", queueMessage));
                }
                else
                {
                    using (var conn = new SqlConnection(dbcn))
                    {
                        await conn.OpenAsync();

                        using (var command = new SqlCommand("INSERT INTO DataSegments(temperature, humidity, Datetime, sigfoxId) values(@temperature, @humidity, @Datetime, @sigfoxId)"))
                        {
                            command.Connection = conn;
                            command.Parameters.AddWithValue("@temperature", jsonnew.Temperature);
                            command.Parameters.AddWithValue("@humidity", jsonnew.Humidity);
                            command.Parameters.AddWithValue("@Datetime", DateTime.Now);
                            command.Parameters.AddWithValue("@sigfoxId", jsonnew.Device);

                            command.ExecuteNonQuery();
                        }
                    }
                }



                //if (toAppend.Length + data.Length > MAX_BLOCK_SIZE || stopwatch.Elapsed > MAX_CHECKPOINT_TIME)
                //{
                //    await AppendAndCheckpoint(context);
                //}
                //await toAppend.WriteAsync(data, 0, data.Length);

                //Console.WriteLine(string.Format("Message received.  Partition: '{0}', Data: '{1}'",
                //  context.Lease.PartitionId, Encoding.UTF8.GetString(data)));
            }
        }

        private async Task AppendAndCheckpoint(PartitionContext context)
        {
            var blockIdString = String.Format("startSeq:{0}", currentBlockInitOffset.ToString("0000000000000000000000000"));
            var blockId = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(blockIdString));
            toAppend.Seek(0, SeekOrigin.Begin);
            byte[] md5 = MD5.Create().ComputeHash(toAppend);
            toAppend.Seek(0, SeekOrigin.Begin);

            var blobName = String.Format("iothubd2c_{0}", context.Lease.PartitionId);
            var currentBlob = blobContainer.GetBlockBlobReference(blobName);

            if (await currentBlob.ExistsAsync())
            {
                await currentBlob.PutBlockAsync(blockId, toAppend, Convert.ToBase64String(md5));
                var blockList = await currentBlob.DownloadBlockListAsync();
                var newBlockList = new List<string>(blockList.Select(b => b.Name));

                if (newBlockList.Count() > 0 && newBlockList.Last() != blockId)
                {
                    newBlockList.Add(blockId);
                    WriteHighlightedMessage(String.Format("Appending block id: {0} to blob: {1}", blockIdString, currentBlob.Name));
                }
                else
                {
                    WriteHighlightedMessage(String.Format("Overwriting block id: {0}", blockIdString));
                }
                await currentBlob.PutBlockListAsync(newBlockList);
            }
            else
            {
                await currentBlob.PutBlockAsync(blockId, toAppend, Convert.ToBase64String(md5));
                var newBlockList = new List<string>();
                newBlockList.Add(blockId);
                await currentBlob.PutBlockListAsync(newBlockList);

                WriteHighlightedMessage(String.Format("Created new blob", currentBlob.Name));
            }

            toAppend.Dispose();
            toAppend = new MemoryStream(MAX_BLOCK_SIZE);

            // checkpoint.
            await context.CheckpointAsync();
            WriteHighlightedMessage(String.Format("Checkpointed partition: {0}", context.Lease.PartitionId));

            currentBlockInitOffset = long.Parse(context.Lease.Offset);
            stopwatch.Restart();
        }

        private void WriteHighlightedMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private CustomSigfoxMessage DeserializeMessage(byte[] data)
        {
            var datastr = Encoding.UTF8.GetString(data);

            var json = JsonConvert.DeserializeObject<SigfoxMessage>(datastr);

            var bytes = GetBytes(json.Data);

            if (bytes[0] == 1)
            {
                var temperature = ParseTemperature(bytes);
                var humidity = ParseHumidity(bytes);

                return new CustomSigfoxMessage
                {
                    Device = json.Device,
                    Time = json.Time,
                    Duplicate = json.Duplicate,
                    Snr = json.Snr,
                    Station = json.Station,
                    Data = json.Data,
                    AvgSnr = json.AvgSnr,
                    Lat = json.Lat,
                    Lng = json.Lng,
                    Rssi = json.Rssi,
                    SegNumber = json.SegNumber,
                    MessageType = "senzory",
                    Temperature = temperature,
                    Humidity = humidity,
                };
            }

            return new CustomSigfoxMessage
            {
                Device = json.Device,
                Time = json.Time,
                Duplicate = json.Duplicate,
                Snr = json.Snr,
                Station = json.Station,
                Data = json.Data,
                AvgSnr = json.AvgSnr,
                Lat = json.Lat,
                Lng = json.Lng,
                Rssi = json.Rssi,
                SegNumber = json.SegNumber,
                MessageType = "alarm",
            };
        }

        private byte[] GetBytes(string data)
        {
            return Enumerable.Range(0, data.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(data.Substring(x, 2), 16))
                     .ToArray();
        }

        private string ParseTemperature(byte[] data)
        {
            return (data[2] - 100).ToString();
        }

        private string ParseHumidity(byte[] data)
        {
            return data[3].ToString();
        }
    }

    class SigfoxMessage
    {
        public string Device;
        public string Time;
        public string Duplicate;
        public string Snr;
        public string Station;
        public string Data;
        public string AvgSnr;
        public string Lat;
        public string Lng;
        public string Rssi;
        public string SegNumber;
    }

    class CustomSigfoxMessage : SigfoxMessage
    {
        public string MessageType;
        public string Temperature;
        public string Humidity;
    }
}
