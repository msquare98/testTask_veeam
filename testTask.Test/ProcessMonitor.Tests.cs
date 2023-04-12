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
        [Test]
        public async Task Monitor_ProcessExistsAndMaxLifetimeExceeded_KillsProcessAndLogs()
        {
            // Arrange
            var maxLifetimeMinutes = 1;
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = cancellationTokenSource.Token;
            var processMonitor = new ProcessMonitor(processName, maxLifetimeMinutes, monitoringFrequencyMinutes, cancellationToken);

            // Delete previous log files
            var logFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), $"log_{processName}_*.txt");
            if (logFiles.Length > 0)
            {
                foreach (var file in logFiles)
                {
                    File.Delete(file);
                }
            }
            // Act
            var process = Process.Start(processName);
            var task = Task.Run(() => processMonitor.Start());

            //await Task.Delay(TimeSpan.FromSeconds(5));
            process.Refresh();
            process.WaitForInputIdle();
            await Task.Delay(TimeSpan.FromMinutes(maxLifetimeMinutes)); // wait for the process to run for at least maxLifetimeMinutes
            cancellationTokenSource.Cancel();

            // Assert
            Assert.That(process.HasExited, Is.False);
            logFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), $"log_{processName}_*.txt");
            Assert.That(logFiles, Has.Length.EqualTo(1));
            var logContent = File.ReadAllText(logFiles[0]);
            StringAssert.Contains($"Process '{processName}' was killed at", logContent);
        }





        [Test]
        public async Task Monitor_ProcessDoesNotExist_DoesNotKillProcess()
        {
            // Arrange
            var maxLifetimeMinutes = 1;
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = cancellationTokenSource.Token;
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
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = cancellationTokenSource.Token;
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
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = cancellationTokenSource.Token;
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
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = cancellationTokenSource.Token;
            var processMonitor = new ProcessMonitor(processName, maxLifetimeMinutes, monitoringFrequencyMinutes, cancellationToken);
            var process = Process.Start(processName);
            var task = Task.Run(() => processMonitor.Start());
            // Act
            await task;
            process.Refresh();
            process.WaitForInputIdle();
            await Task.Delay(TimeSpan.FromSeconds(5));
            process.Refresh();
            process.WaitForInputIdle();
            cancellationTokenSource.Cancel();

            // Assert
            var logFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), $"log_{processName}_*.txt");
            Assert.That(logFiles, Has.Length.EqualTo(1));
            Assert.IsFalse(process.HasExited);
        }


        [Test]
        public async Task Monitor_ProcessAlreadyExited_LogsProcessExit()
        {
            // Arrange
            var maxLifetimeMinutes = 1;
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = cancellationTokenSource.Token;
            var processMonitor = new ProcessMonitor(processName, maxLifetimeMinutes, monitoringFrequencyMinutes, cancellationToken);

            // Act
            var process = Process.Start(processName);
            var task = Task.Run(() => processMonitor.Start());

            await Task.Delay(TimeSpan.FromSeconds(5));
            process.Refresh();
            process.WaitForInputIdle();
            await Task.Delay(TimeSpan.FromSeconds(5));
            process.Kill();
            process.WaitForExit();

            await task; // Wait for processMonitor.Start() to complete before continuing

            cancellationTokenSource.Cancel();

            // Assert
            var logFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), $"log_{processName}_*.txt");
            Assert.That(logFiles, Has.Length.EqualTo(1));
            var logContent = File.ReadAllText(logFiles[0]);
            StringAssert.Contains($"Process '{processName}' has already exited", logContent);
        }


        [Test]
        public async Task Monitor_ProcessNotFound_LogsProcessNotFound()
        {
            // Arrange
            var maxLifetimeMinutes = 1;
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = cancellationTokenSource.Token;
            var processMonitor = new ProcessMonitor(processName, maxLifetimeMinutes, monitoringFrequencyMinutes, cancellationToken);

            // Act
            var process = Process.Start("notepad.exe"); // Start a different process
            var task = Task.Run(() => processMonitor.Start());
            await Task.Delay(TimeSpan.FromSeconds(5));
            process.Refresh();
            process.WaitForInputIdle();
            await Task.Delay(TimeSpan.FromSeconds(5));
            cancellationTokenSource.Cancel();

            // Assert
            var logFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), $"log_{processName}_*.txt");
            Assert.That(logFiles, Has.Length.EqualTo(1));
            var logContent = File.ReadAllText(logFiles[0]);
            StringAssert.Contains($"Process '{processName}' not found", logContent);
        }


    }
}
