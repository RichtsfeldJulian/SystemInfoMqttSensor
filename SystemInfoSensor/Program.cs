﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;

namespace SystemInfoSensor
{
    class Program
    {
        private const string Topic = "systemInfo";
        private const string AlarmTopic = "alarmInfo";

        private static PerformanceCounter cpuCounter;
        private static PerformanceCounter ramCounter;
        private static PerformanceCounter frequencyCounter;
        private static PerformanceCounter powerCounter;
        private static MQQTClient client =  new MQQTClient("185.239.238.179");
        private static PubSubClient pubSubClient = new PubSubClient();

        //Auslesen der Systemdaten über die PerformanceCounter
        static void InitialisePerformanceCounter()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            frequencyCounter = new PerformanceCounter("Processor information","Processor frequency","_Total");
            powerCounter = new PerformanceCounter("Processor information", "% Processor performance", "_Total");
        }

        //Berechnen des RAM's
        static int GetPhysicalMemory()
        {
            var gcMemoryInfo = GC.GetGCMemoryInfo();
            var installedMemory = gcMemoryInfo.TotalAvailableMemoryBytes;
            double physicalMemory = (double)installedMemory / 1048576.0;
            return Convert.ToInt32(Math.Round(physicalMemory));
        }

        
        static async Task Main(string[] args)
        {
            InitialisePerformanceCounter();
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            while (true)
            {
                Console.WriteLine(cpuCounter.NextValue() + " : CPU-Auslatung in Prozent");
                Console.WriteLine(ramCounter.NextValue() + " : RAM-Auslastung in MB");
                Console.WriteLine(frequencyCounter.NextValue() * (powerCounter.NextValue()/100) + " : CPU in GHZ");
                Console.WriteLine(GetPhysicalMemory() + " : Total RAM in MB");

                System.Threading.Thread.Sleep(1000);

                //Zusammenstellen des Payloads mit den Daten des Systems
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
                await pubSubClient.PublishMessagesAsync(Topic,payload);
                client.Publish(Topic,payload);
            }
        }

        //Sollte der Sensor geschlossen werden, wird ein Event getriggerd, der Sensor meldet sich ab und sendet einen letzte Nachricht mit dem Maschinennamen.
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        private static async Task publishConnectionClosed(string machineName)
        {
            client.Publish("connectionClosed", machineName);
            await pubSubClient.PublishMessagesAsync("connectionClosed", machineName);
        }

        private static bool Handler(CtrlType sig)
        {
            string machineName = JsonConvert.SerializeObject(new
            {
                Name = Environment.MachineName,
            });
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
#pragma warning disable CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                    publishConnectionClosed(machineName);
#pragma warning restore CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                    break;
                case CtrlType.CTRL_LOGOFF_EVENT:
#pragma warning disable CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                    publishConnectionClosed(machineName);
#pragma warning restore CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                    break;
                case CtrlType.CTRL_SHUTDOWN_EVENT:
#pragma warning disable CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                    publishConnectionClosed(machineName);
#pragma warning restore CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                    break;
                case CtrlType.CTRL_CLOSE_EVENT:
#pragma warning disable CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                    publishConnectionClosed(machineName);
#pragma warning restore CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                    break;
                case CtrlType.CTRL_BREAK_EVENT:
#pragma warning disable CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                    publishConnectionClosed(machineName);
#pragma warning restore CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
