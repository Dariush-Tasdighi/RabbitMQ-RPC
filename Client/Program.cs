using RabbitMQ.Client;

namespace Application
{
	public static class Program
	{
		static Program()
		{
		}

		private static async System.Threading.Tasks.Task Main()
		{
			System.Console.WriteLine(" [x] Sending RPC");

			int sleepMilliseconds = 100;

			var client =
				new Dtx.RabbitMQ.RpcClient();

			while (1 == 1)
			{
				var stopWatch =
					new System.Diagnostics.Stopwatch();

				stopWatch.Start();

				System.Threading.Thread.Sleep
					(millisecondsTimeout: sleepMilliseconds);

				var result =
					await client.CallAsync(System.Guid.NewGuid().ToString());

				stopWatch.Stop();

				var extraTime =
					new System.TimeSpan
					(days: 0, hours: 0, minutes: 0, seconds: 0, milliseconds: sleepMilliseconds);

				var elapsedTime =
					stopWatch.Elapsed - extraTime;

				string elapsedSecondsString =
					elapsedTime.Seconds.ToString().PadLeft(totalWidth: 2, paddingChar: '_');

				string elapsedMillisecondsString =
					elapsedTime.Milliseconds.ToString().PadLeft(totalWidth: 4, paddingChar: '_');

				string message =
					$"{ result } - Seconds: { elapsedSecondsString } - Milliseconds: { elapsedMillisecondsString }";

				System.Console.WriteLine(message);
			}

			//client.Close();
		}
	}
}
