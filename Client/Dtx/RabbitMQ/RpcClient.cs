using RabbitMQ.Client;

namespace Dtx.RabbitMQ
{
	public class RpcClient
	{
		public const string QueueName = "rpc_queue";

		public RpcClient() : base()
		{
			// **************************************************
			CallbackMapper =
				new System.Collections.Concurrent.ConcurrentDictionary
				<string, System.Threading.Tasks.TaskCompletionSource<string>>();
			// **************************************************

			// **************************************************
			var factory =
				new global::RabbitMQ.Client.ConnectionFactory() { HostName = "localhost" };

			Connection =
				factory.CreateConnection();

			Channel =
				Connection.CreateModel();

			Consumer =
				new global::RabbitMQ.Client.Events.EventingBasicConsumer(model: Channel);
			// **************************************************

			// **************************************************
			// using RabbitMQ.Client;
			// Declare a server-named queue
			ReplyQueueName =
				Channel.QueueDeclare(queue: string.Empty).QueueName;
			// **************************************************

			Consumer.Received += (model, eventArgs) =>
			{
				// **************************************************
				bool result =
					CallbackMapper.TryRemove(key: eventArgs.BasicProperties.CorrelationId,
					value: out System.Threading.Tasks.TaskCompletionSource<string> taskCompletionSource);

				if (result == false)
				{
					return;
				}
				// **************************************************

				// **************************************************
				var body =
					eventArgs.Body.ToArray();

				var response =
					System.Text.Encoding.UTF8.GetString(body);
				// **************************************************

				// **************************************************
				taskCompletionSource.TrySetResult(result: response);
				// **************************************************
			};
		}

		private string ReplyQueueName { get; }

		private global::RabbitMQ.Client.IModel Channel { get; }

		private global::RabbitMQ.Client.IConnection Connection { get; }

		private global::RabbitMQ.Client.Events.EventingBasicConsumer Consumer { get; }

		private System.Collections.Concurrent.ConcurrentDictionary
			<string, System.Threading.Tasks.TaskCompletionSource<string>> CallbackMapper
		{ get; }

		public System.Threading.Tasks.Task<string>
			CallAsync(string message, System.Threading.CancellationToken cancellationToken = default)
		{
			// **************************************************
			var correlationId =
				System.Guid.NewGuid().ToString();

			var taskCompletionSource =
				new System.Threading.Tasks.TaskCompletionSource<string>();

			CallbackMapper.TryAdd
				(key: correlationId, value: taskCompletionSource);
			// **************************************************

			// **************************************************
			// **************************************************
			// **************************************************
			var properties =
				Channel.CreateBasicProperties();

			properties.ReplyTo = ReplyQueueName;
			properties.CorrelationId = correlationId;
			// **************************************************

			// **************************************************
			var messageBytes =
				System.Text.Encoding.UTF8.GetBytes(message);
			// **************************************************

			// **************************************************
			// using RabbitMQ.Client;
			Channel.BasicPublish
				(exchange: string.Empty,
				routingKey: QueueName,
				basicProperties: properties,
				body: messageBytes);

			// using RabbitMQ.Client;
			Channel.BasicConsume
				(consumer: Consumer,
				queue: ReplyQueueName,
				autoAck: true);
			// **************************************************
			// **************************************************
			// **************************************************

			// **************************************************
			cancellationToken
				.Register(() => CallbackMapper.TryRemove(key: correlationId, value: out var temp));
			// **************************************************

			return taskCompletionSource.Task;
		}

		public void Close()
		{
			if (Channel != null)
			{
				Channel.Close();
				Channel.Dispose();
			}

			if (Connection != null)
			{
				Connection.Close();
				Connection.Dispose();
			}
		}
	}
}
