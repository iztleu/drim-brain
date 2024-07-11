using Confluent.Kafka;
using Confluent.Kafka.Admin;

var config = new AdminClientConfig { BootstrapServers = "localhost:9092" };

using var adminClient = new AdminClientBuilder(config).Build();

try
{
    await adminClient.CreateTopicsAsync(new TopicSpecification[]
    {
        new()
        {
            Name = "crypto-deposit-created",
            NumPartitions = 10,
            ReplicationFactor = 1,
        }
    });

    Console.WriteLine("Topics created successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred creating topics:\n{ex}");
}
