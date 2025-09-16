using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerPodService
{
    public class Worker
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();
        private readonly List<string> _batch = new List<string>();

        private readonly int _batchSize = 10;
        private readonly TimeSpan _batchWindow = TimeSpan.FromMinutes(5);
        private DateTime _lastFlush = DateTime.UtcNow;

        public async Task RunAsync(CancellationToken token)
        {
            Console.WriteLine("Worker started.");

            // Background task that simulates incoming messages
            _ = Task.Run(() => ProduceMessages(token), token);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (_queue.TryDequeue(out var msg))
                    {
                        _batch.Add(msg);
                    }

                    // Flush if batch full or timeout
                    if (_batch.Count >= _batchSize ||
                        (DateTime.UtcNow - _lastFlush) > _batchWindow)
                    {
                        if (_batch.Count > 0)
                        {
                            await SendBatchAsync(_batch, token);
                            _batch.Clear();
                            _lastFlush = DateTime.UtcNow;
                        }
                    }

                    await Task.Delay(1000, token);
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Worker stopping due to cancellation.");
            }
            finally
            {
                Console.WriteLine("Worker stopped.");
            }
        }

        private async Task ProduceMessages(CancellationToken token)
        {
            var rnd = new Random();

            while (!token.IsCancellationRequested)
            {
                string msg = $"Message-{rnd.Next(1000, 9999)}";
                _queue.Enqueue(msg);

                await Task.Delay(TimeSpan.FromSeconds(30), token); // produce every 30s
            }
        }

        private async Task SendBatchAsync(List<string> batch, CancellationToken token)
        {
            try
            {
                var json = JsonSerializer.Serialize(batch);
                var response = await _httpClient.PostAsync(
                    "https://webhook.site/65f060f1-0e29-43d6-81d2-c1176d89b046",
                    new StringContent(json, Encoding.UTF8, "application/json"),
                    token);

                Console.WriteLine($"[BATCH SENT] {batch.Count} messages -> Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send webhook: {ex.Message}");
            }
        }
    }
}
