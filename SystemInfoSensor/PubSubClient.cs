using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;

namespace SystemInfoSensor
{
    class PubSubClient
    {
        private static string topic = "systemInfo";
        private static string projectId = "systemmonitoring-294918";
        private PublisherServiceApiClient publisher;
        public PubSubClient()
        {
            publisher = PublisherServiceApiClient.Create();
        }

        /*public Topic CreateTopic(string projectId, string topicId)
        {
            var topicName = TopicName.FromProjectTopic(projectId, topicId);
            Topic topic = null;

            try
            {
                topic = publisher.CreateTopic(topicName);
                Console.WriteLine($"Topic {topic.Name} created.");
            }
            catch (RpcException e) when (e.Status.StatusCode == StatusCode.AlreadyExists)
            {
                Console.WriteLine($"Topic {topicName} already exists.");
            }
            return topic;
        }*/
        public async Task<int> PublishMessagesAsync(string topicId, string message)
        {
            TopicName topicName = TopicName.FromProjectTopic(projectId, topicId);
            PublisherClient publisher = await PublisherClient.CreateAsync(topicName);

            int publishedMessageCount = 0;
            
            try
            {
                string res = await publisher.PublishAsync(message);
                Console.WriteLine($"Published message {res}");
                Interlocked.Increment(ref publishedMessageCount);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"An error ocurred when publishing message: {exception.Message}");
            }
            return publishedMessageCount;
        }
    }
}
