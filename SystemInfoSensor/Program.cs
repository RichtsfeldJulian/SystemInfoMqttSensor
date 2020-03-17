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
        static PerformanceCounter frequencyCounter;
        private static PerformanceCounter powerCounter;
        static void InitialisePerformanceCounter()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            frequencyCounter = new PerformanceCounter("Processor information","Processor frequency","_Total");
            powerCounter = new PerformanceCounter("Processor information", "% Processor performance", "_Total");
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
                Console.WriteLine(frequencyCounter.NextValue() * (powerCounter.NextValue()/100) + " : CPU in GHZ");

                System.Threading.Thread.Sleep(1000);

                string payload = JsonConvert.SerializeObject(new SystemInfo
                {
                    Id = Environment.MachineName,
                    Cpu = new Cpu
                    {
                        Baseclock = Convert.ToInt32(Math.Round(frequencyCounter.NextValue())),
                        Currentclock = Convert.ToInt32(Math.Round(frequencyCounter.NextValue() * (powerCounter.NextValue() / 100))),
                        Utilisation = Convert.ToInt32(Math.Round(cpuCounter.NextValue()))
                    },
                    Ram = new Ram
                    {
                        Used = Math.Round(ramCounter.NextValue()),
                        Max = 16009
                    }
                });

                client.Publish(TOPIC, Encoding.UTF8.GetBytes(payload) , MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
            }
        }
    }
}
