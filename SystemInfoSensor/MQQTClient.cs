using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace SystemInfoSensor
{
    class MQQTClient
    {
        private static MqttClient _client { get; set; }

        public MQQTClient(string ip)
        {
            _client = new MqttClient(ip);
            _client.Connect(new Guid().ToString());
        }

        public void CloseConnection()
        {
            _client.Disconnect();
        }

        public void  Publish(string topic, string message)
        {
            _client.Publish(topic, Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        }
    }
}
