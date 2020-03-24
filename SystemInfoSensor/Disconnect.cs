using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace SystemInfoSensor
{
    class Disconnect
    {
        public static MqttClient Client { get; set; }

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        public delegate bool HandlerRoutine(MqttClient client);

        public static bool InspectControlType(MqttClient client)
        {
            Client = client;
            client.Disconnect();
            return true;
        }

        public static void Client_ConnectionClosed(object sender, EventArgs e)
        {
            string dissString = JsonConvert.SerializeObject(new 
            {
                Name = Environment.MachineName,
            });
            Client.Publish("connectionClosed", Encoding.UTF8.GetBytes(dissString), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
                false);
        }
    }
}
