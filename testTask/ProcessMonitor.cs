using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ProcessKiller
{
    public class ProcessMonitor
    {
        private readonly string processName;
        private readonly int maxLifetimeMinutes;
        private readonly int monitoringFrequencyMinutes;
        private readonly CancellationToken cancellationToken;

        public ProcessMonitor(string processName, int maxLifetimeMinutes, int monitoringFrequencyMinutes, CancellationToken cancellationToken)
        {
            this.processName = processName;
            this.maxLifetimeMinutes = maxLifetimeMinutes;
            this.monitoringFrequencyMinutes = monitoringFrequencyMinutes;
            this.cancellationToken = cancellationToken;
        }

        public void Start()
        {
            Console.WriteLine($"Monitoring process '{processName}' for {maxLifetimeMinutes} minutes with a frequency of {monitoringFrequencyMinutes} minutes.");

            while (!cancellationToken.IsCancellationRequested)
            {
                var processes = Process.GetProcessesByName(processName);
                var now = DateTime.Now;

                foreach (var process in processes)
                {
                    if ((now - process.StartTime).TotalMinutes > maxLifetimeMinutes)
                    {
                        Console.WriteLine($"Killing process {process.ProcessName} (PID {process.Id}) because it exceeded its max lifetime of {maxLifetimeMinutes} minutes.");
                        process.Kill();

                        // Generate the log file name
                        var fileName = $"log_{processName}_{process.StartTime:yyyyMMddHHmmss}.txt";

                        // Create the log file in the current directory
                        using var writer = File.CreateText(fileName);
                        writer.WriteLine($"Process '{processName}' was killed at {now} because it exceeded its max lifetime of {maxLifetimeMinutes} minutes.");
                        Console.WriteLine($"Log file generated: {fileName}");
                    }
                }

                Console.Write(".");
                cancellationToken.WaitHandle.WaitOne(monitoringFrequencyMinutes * 60 * 1000);

                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                {
                    Console.WriteLine("\nStopping process monitor.");
                    return;
                }
            }

            Console.WriteLine("Process monitor ended.");
        }

        static void Main(string[] args)
        {
            if (args.Length != 3 || !int.TryParse(args[1], out int maxLifetimeMinutes) || !int.TryParse(args[2], out int monitoringFrequencyMinutes))
            {
                Console.WriteLine("Usage: ProcessMonitor [process name] [max lifetime in minutes] [monitoring frequency in minutes]");
                return;
            }

            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(maxLifetimeMinutes + monitoringFrequencyMinutes));
            var token = cts.Token;
            var processMonitor = new ProcessMonitor(args[0], maxLifetimeMinutes, monitoringFrequencyMinutes, token);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
            };

            processMonitor.Start();
        }
    }
}
