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
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.WriteLine("Podservice starting...");

            using var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Stopping Podservice (CTRL+C)...");
                e.Cancel = true;
                if (!cts.IsCancellationRequested) cts.Cancel();
            };

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                Console.WriteLine("Stopping Podservice (ProcessExit)...");
                if (!cts.IsCancellationRequested) cts.Cancel();
            };

            var worker = new Worker();
            var workerTask = Task.Run(() => worker.RunAsync(cts.Token));

            Console.WriteLine("Worker loop started.");

            await workerTask;

            Console.WriteLine("Podservice stopped.");
        }
    }
}
