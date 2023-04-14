using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace ProcessKiller.Tests
{
    [TestFixture]
    public class ProcessMonitorTests
    {
        private const string processName = "notepad";
        private const int monitoringFrequencyMinutes = 1;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        [SetUp]
        public void Setup()
        {
            cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            cancellationToken = cancellationTokenSource.Token;
        }


        
        [Test]
        public async Task Monitor_ProcessExistsAndMaxLifetimeExceeded_KillsProcessAndLogs()
        {
            //For this test to pass, a process(notepad) should already be opened an exceeded its lifetime
            // Arrange
            var maxLifetimeMinutes = 1;
            var monitoringFrequencyMinutes = 1;
            var processMonitor = new ProcessMonitor(processName, maxLifetimeMinutes, monitoringFrequencyMinutes, cancellationToken);
            var logFilesBefore = Directory.GetFiles(Directory.GetCurrentDirectory(), $"log_{processName}_*.txt");
            // Act
            var task = Task.Run(() => processMonitor.Start());
            await task;
            cancellationTokenSource.Cancel();

            var logFilesAfter = Directory.GetFiles(Directory.GetCurrentDirectory(), $"log_{processName}_*.txt");
            //Assert
            Assert.That(logFilesAfter.Length, Is.EqualTo(logFilesBefore.Length+1));
        }



        [Test]
        public async Task Monitor_ProcessDoesNotExist_DoesNotKillProcess()
        {
            // Arrange
            var maxLifetimeMinutes = 1;
            var processMonitor = new ProcessMonitor(processName, maxLifetimeMinutes, monitoringFrequencyMinutes, cancellationToken);
            var task = Task.Run(() => processMonitor.Start());
            // Act
            await task;
            cancellationTokenSource.Cancel();
            // Assert
            Assert.Pass();
        }

        [Test]
        public async Task Monitor_ProcessExistsAndMaxLifetimeNotExceeded_DoesNotKillProcess()
        {
            // Arrange
            var maxLifetimeMinutes = 2;
            var processMonitor = new ProcessMonitor(processName, maxLifetimeMinutes, monitoringFrequencyMinutes, cancellationToken);
            // Act
            var process = Process.Start(processName);
            var task = Task.Run(() => processMonitor.Start());
            await task;
            process.Refresh();
            process.WaitForInputIdle();
            await task;
            cancellationTokenSource.Cancel();

            // Assert
            Assert.Pass();
        }

        [Test]
        public async Task Monitor_ProcessIsCancelled_StopsProcessMonitor()
        {
            // Arrange
            var maxLifetimeMinutes = 1;
            var processMonitor = new ProcessMonitor(processName, maxLifetimeMinutes, monitoringFrequencyMinutes, cancellationToken);
            // Act
            var task = Task.Run(() => processMonitor.Start());
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));
            await task;
            // Assert
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public async Task Monitor_ProcessExistsAndMaxLifetimeNotExceeded_DoesNotKillProcessOrLog()
        {
            // Arrange
            var maxLifetimeMinutes = 1;
            KillAlreadyRunningProcess();
            var processMonitor = new ProcessMonitor(processName, maxLifetimeMinutes, monitoringFrequencyMinutes, cancellationToken);
            var logFilesbefore = Directory.GetFiles(Directory.GetCurrentDirectory(), $"log_{processName}_*.txt");
            var process = Process.Start(processName);
            var task = Task.Run(() => processMonitor.Start());
            // Act
            await Task.Delay(TimeSpan.FromSeconds(5));
            process.Refresh();
            process.WaitForInputIdle();
            await Task.Delay(TimeSpan.FromMinutes(maxLifetimeMinutes + 1));
            process.Refresh();
            process.WaitForInputIdle();
            cancellationTokenSource.Cancel();
            var logFilesAfter = Directory.GetFiles(Directory.GetCurrentDirectory(), $"log_{processName}_*.txt");

            // Assert
            Assert.IsFalse(process.HasExited);
            Assert.That(logFilesAfter.Length, Is.EqualTo(logFilesbefore.Length));
        }


        [Test]
        public async Task Monitor_ProcessNotFound_LogsProcessNotFound()
        {
            // Arrange
            var maxLifetimeMinutes = 1;
            var processMonitor = new ProcessMonitor(processName, maxLifetimeMinutes, monitoringFrequencyMinutes, cancellationToken);
            //DeleteOldLogFiles();
            var logFilesBefore = Directory.GetFiles(Directory.GetCurrentDirectory(), $"log_{processName}_*.txt");
            // Act
            var task = Task.Run(() => processMonitor.Start());
            await task;
            cancellationTokenSource.Cancel();

            // Assert
            var logFilesAfter = Directory.GetFiles(Directory.GetCurrentDirectory(), $"log_{processName}_*.txt");
            Assert.That(logFilesAfter.Length, Is.EqualTo(logFilesBefore.Length));

        }


        private static void KillAlreadyRunningProcess()
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes.Any())
            {
                foreach (var pro in processes)
                {
                    pro.Kill();
                }
            }
        }
    }
}
