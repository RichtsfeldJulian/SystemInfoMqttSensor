using System;
using System.Diagnostics;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace SystemInfoSensor
{
    class Program
    {
        static PerformanceCounter cpuCounter;
        private static PerformanceCounter ramCounter;
        static void InitialisierePerformanceCounter() // Initialisieren
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }
        static void Main(string[] args)
        {

            InitialisierePerformanceCounter();
            MqttClient client = new MqttClient("185.239.238.179");
            byte code = client.Connect(Guid.NewGuid().ToString());

            while (true)
            {
                Console.WriteLine(cpuCounter.NextValue() + " : CPU");
                Console.WriteLine(ramCounter.NextValue() + " : RAM");
                System.Threading.Thread.Sleep(1000);

                client.Publish("systemInfo",
                    Encoding.UTF8.GetBytes(cpuCounter.NextValue() + " : " + ramCounter.NextValue()), // message body
                    MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
                    true);


            }
        }
    }
}
