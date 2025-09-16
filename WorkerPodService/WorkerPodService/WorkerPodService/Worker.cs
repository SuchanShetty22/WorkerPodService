using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerPodService
{
    public class Worker
    {
        private readonly int intervalSeconds = 30; // ?? Changed from 10 to 30
        private readonly string webhookUrl = "https://webhook.site/ad93c79a-ab56-4d76-bbca-1b5e36f7cfda";
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task RunAsync(CancellationToken token)
        {
            Console.WriteLine("Worker started.");

            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // Perform GET request to webhook
                        var response = await httpClient.GetAsync(webhookUrl, token);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Webhook GET success");
                        }
                        else
                        {
                            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Webhook GET failed: {response.StatusCode}");
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine("TaskCanceledException during GET (shutting down worker)...");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WORKER ERROR] Webhook request failed: {ex}");
                    }

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), token);
                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine("TaskCanceledException during delay (shutting down worker)...");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FATAL ERROR in RunAsync] {ex}");
            }
            finally
            {
                Console.WriteLine("Worker stopped.");
            }
        }
    }
}
