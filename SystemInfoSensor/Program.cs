﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace SystemInfoSensor
{
    class Program
    {
        private const string TOPIC = "systemInfo";
        private const string ALARMTOPIC = "alarmInfo";

        private static PerformanceCounter cpuCounter;
        private static PerformanceCounter ramCounter;
        private static PerformanceCounter frequencyCounter;
        private static PerformanceCounter powerCounter;

        static void InitialisePerformanceCounter()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            frequencyCounter = new PerformanceCounter("Processor information","Processor frequency","_Total");
            powerCounter = new PerformanceCounter("Processor information", "% Processor performance", "_Total");
        }

        static int GetPhysicalMemory()
        {
            var gcMemoryInfo = GC.GetGCMemoryInfo();
            var installedMemory = gcMemoryInfo.TotalAvailableMemoryBytes;
            double physicalMemory = (double)installedMemory / 1048576.0;
            return Convert.ToInt32(Math.Round(physicalMemory));

        }

        
        static void Main(string[] args)
        {
            Disconnect.HandlerRoutine hr = new Disconnect.HandlerRoutine(Disconnect.InspectControlType);
            Disconnect.SetConsoleCtrlHandler(hr, true);
            InitialisePerformanceCounter();
            MqttClient client = new MqttClient("185.239.238.179");
            byte code = client.Connect(Guid.NewGuid().ToString());
            client.ConnectionClosed += Disconnect.Client_ConnectionClosed;   

            while (true)
            {
                Console.WriteLine(cpuCounter.NextValue() + " : CPU-Auslatung in Prozent");
                Console.WriteLine(ramCounter.NextValue() + " : RAM-Auslastung in MB");
                Console.WriteLine(frequencyCounter.NextValue() * (powerCounter.NextValue()/100) + " : CPU in GHZ");
                Console.WriteLine(GetPhysicalMemory() + " : Total RAM in MB");

                System.Threading.Thread.Sleep(1000);

                string payload = JsonConvert.SerializeObject(new Measurement
                {
                    SystemInfo = new SystemInfo { 
                        Name = Environment.MachineName, 
                        Cpu = new Cpu
                        {
                            Baseclock = Convert.ToInt32(Math.Round(frequencyCounter.NextValue())),
                            Currentclock = Convert.ToInt32(Math.Round(frequencyCounter.NextValue() * (powerCounter.NextValue() / 100))),
                            Utilisation = Convert.ToInt32(Math.Round(cpuCounter.NextValue()))
                        },
                        Ram = new Ram
                        {
                            Used = Math.Round(ramCounter.NextValue()),
                            Max = GetPhysicalMemory()
                        }
                    },
                    Timestamp = DateTime.Now
                });

               if (cpuCounter.NextValue() > 90)
               {
                   string alarmString = JsonConvert.SerializeObject(new Warning
                   {
                       Name = Environment.MachineName,
                       Description = "Achtung! CPU-Auslastung über 90% !"
                   });
                   client.Publish(ALARMTOPIC, Encoding.UTF8.GetBytes(alarmString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
               }

               client.Publish(TOPIC, Encoding.UTF8.GetBytes(payload) , MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
            }
        }
    }
}
