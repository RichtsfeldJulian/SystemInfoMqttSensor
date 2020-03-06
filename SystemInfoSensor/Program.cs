using System;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace SystemInfoSensor
{
    class Program
    {
        private const string TOPIC = "systemInfo";

        static PerformanceCounter cpuCounter;
        static PerformanceCounter ramCounter;
        static void InitialisePerformanceCounter()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }
        static void Main(string[] args)
        {

            InitialisePerformanceCounter();
            MqttClient client = new MqttClient("185.239.238.179");
            byte code = client.Connect(Guid.NewGuid().ToString());

            while (true)
            {
                Console.WriteLine(cpuCounter.NextValue() + " : CPU");
                Console.WriteLine(ramCounter.NextValue() + " : RAM");
                System.Threading.Thread.Sleep(1000);

                string payload = JsonConvert.SerializeObject(new SystemInfo
                {
                    Id = Environment.MachineName,
                    Cpu = Math.Round(cpuCounter.NextValue()).ToString(),
                    Ram = Math.Round(ramCounter.NextValue()).ToString()
                });

                client.Publish(TOPIC, Encoding.UTF8.GetBytes(payload) , MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
            }
        }
    }
}
