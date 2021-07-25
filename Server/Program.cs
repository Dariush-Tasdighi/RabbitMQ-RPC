using RabbitMQ.Client;

namespace Application
{
	public static class Program
	{
		public const string QueueName = "rpc_queue";

		static Program()
		{
		}

		private static void Main()
		{
			// **************************************************
			long messageIndex = 0;
			long serverIndex = 0;

			do
			{
				System.Console.Clear();
				System.Console.Write("Server Index: ");
				string serverIndexString = System.Console.ReadLine();

				try
				{
					serverIndex =
						System.Convert.ToInt32(serverIndexString);
				}
				catch
				{
				}

			} while (serverIndex == 0);
			// **************************************************

			var factory =
				new RabbitMQ.Client.ConnectionFactory() { HostName = "localhost" };

			using (var connection = factory.CreateConnection())
			{
				using (var channel = connection.CreateModel())
				{
					channel.QueueDeclare
						(queue: QueueName,
						durable: false,
						exclusive: false,
						autoDelete: false,
						arguments: null);

					channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

					var consumer =
						new RabbitMQ.Client.Events.EventingBasicConsumer(model: channel);

					// using RabbitMQ.Client;
					channel.BasicConsume
						(queue: QueueName,
						autoAck: false,
						consumer: consumer);

					System.Console.WriteLine($"[x] RPC Server { serverIndex } Started...");

					consumer.Received += (model, eventArgs) =>
					{
						messageIndex++;

						// **************************************************
						var properties =
							eventArgs.BasicProperties;

						var replyProperties =
							channel.CreateBasicProperties();

						replyProperties.CorrelationId = properties.CorrelationId;
						// **************************************************

						string response = null;

						try
						{
							var body =
								eventArgs.Body.ToArray();

							var message =
								System.Text.Encoding.UTF8.GetString(body);

							string messageIndexString =
								messageIndex.ToString().PadLeft(totalWidth: 6, paddingChar: '_');

							//System.Threading.Thread.Sleep(millisecondsTimeout: 1500);
							//System.Threading.Tasks.Task.Delay(millisecondsDelay: 1200);

							string result =
								$"Server: { serverIndex } - Message: { message }- Index: { messageIndexString }";

							System.Console.WriteLine(result);

							response = result;
						}
						catch (System.Exception ex)
						{
							response = 0.ToString();

							System.Console.WriteLine($"Error Message: { ex.Message }");
						}
						finally
						{
							var responseBytes =
								System.Text.Encoding.UTF8.GetBytes(response);

							// using RabbitMQ.Client;
							channel.BasicPublish
								(exchange: string.Empty,
								routingKey: properties.ReplyTo,
								basicProperties: replyProperties,
								body: responseBytes);

							channel.BasicAck
								(deliveryTag: eventArgs.DeliveryTag, multiple: false);
						}
					};

					// این دو دستور ذیل باید دقیقا اینجا نوشته شوند
					System.Console.Write(" Press [ENTER] to exit...");
					System.Console.ReadLine();
				}
			}

			System.Console.WriteLine("Consumer Stoped!");
		}
	}
}
