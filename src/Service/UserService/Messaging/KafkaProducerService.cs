using Confluent.Kafka;
using System.Text.Json;

namespace UserService.Messaging
{
	public class KafkaProducerService
	{
		private readonly IProducer<Null, string> _producer;

		public KafkaProducerService(IConfiguration configuration)
		{
			// Lấy địa chỉ máy chủ Kafka từ cấu hình
			var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

			var config = new ProducerConfig
			{
				BootstrapServers = bootstrapServers
			};

			_producer = new ProducerBuilder<Null, string>(config).Build();
		}

		// Hàm này dùng để bắn tin nhắn lên Loa phát thanh
		public async Task ProduceAsync<T>(string topic, T message)
		{
			var jsonMessage = JsonSerializer.Serialize(message);
			await _producer.ProduceAsync(topic, new Message<Null, string> { Value = jsonMessage });
		}
	}
}