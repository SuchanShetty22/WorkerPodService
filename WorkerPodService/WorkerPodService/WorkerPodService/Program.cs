using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WorkerPodService;

namespace WorkerPodService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Ensure Console output auto-flushes for Kubernetes logs
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput())
            {
                AutoFlush = true
            });

            Console.WriteLine("Worker Pod Service starting...");

            try
            {
                using var cts = new CancellationTokenSource();

                // Handle Ctrl+C
                Console.CancelKeyPress += (sender, e) =>
                {
                    Console.WriteLine("Stopping Worker Pod Service (CTRL+C)...");
                    e.Cancel = true;
                    if (!cts.IsCancellationRequested)
                        cts.Cancel();
                };

                // Handle process exit (SIGTERM in Kubernetes)
                AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                {
                    Console.WriteLine("Stopping Worker Pod Service (ProcessExit)...");
                    if (!cts.IsCancellationRequested)
                        cts.Cancel();
                };

                var worker = new Worker();

                // Start worker in background task
                var workerTask = Task.Run(() => worker.RunAsync(cts.Token));

                Console.WriteLine("Worker loop started.");

                // Keep Main alive until cancellation
                while (!cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(1000, cts.Token);
                }

                // Wait for worker to finish gracefully
                await workerTask;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("TaskCanceledException caught in Main (shutting down)...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FATAL ERROR in Main] {ex}");
            }
            finally
            {
                Console.WriteLine("Worker Pod Service stopped.");
            }
        }
    }
}
