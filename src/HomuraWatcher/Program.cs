using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using HomuraWatcher.Models;
using Newtonsoft.Json;

namespace HomuraWatcher
{
    internal class Program
    {
        private const string HomuraApiUrl = "http://api/api/v1/artist";
        private const int MinutesBetweenPulls = 5;
        private const string SeedFile = "/data/seed.txt";

        private static readonly HttpClient Client = new();

        private static async Task Main()
        {
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.Add("User-Agent", "HomuraWatcher");
            Client.DefaultRequestHeaders.Add("X-ACCESS-TOKEN", Environment.GetEnvironmentVariable("HOMURA_API_TOKEN"));

            await Seed();
            
            while (true)
            {
                Artist[] artists = await Get<Artist[]>();

                foreach (Artist artist in artists)
                {
                    if (artist.Media == null || artist.Media.Length == 0) continue;
                    
                    foreach (long mediaId in artist.Media)
                    {
                        string tweetUrl = $"https://twitter.com/{artist.TwitterUsername}/status/{mediaId}";
                        await SendTelegramMessage(tweetUrl);
                    }

                    artist.LastProcessedTweetId = artist.Media.Max();
                    await Put($"{HomuraApiUrl}/{artist.TwitterId}", artist);
                }
                
                Thread.Sleep(MinutesBetweenPulls * 60 * 1000);
            }
        }

        private static async Task<T> Get<T>()
        {
            while (true)
            {
                try
                {
                    string response = await Client.GetStringAsync(HomuraApiUrl);
                    return JsonConvert.DeserializeObject<T>(response);
                }
                catch (HttpRequestException)
                {
                    continue;
                }
                catch
                {
                    throw;
                }
            }
        }

        private static async Task Post(string url, object body = null)
        {
            while (true)
            {
                try
                {
                    _ = await Client.PostAsJsonAsync(url, body);
                    break;
                }
                catch (HttpRequestException)
                {
                    continue;
                }
                catch
                {
                    throw;
                }
            }
        }

        private static async Task Put(string url, object body)
        {
            while (true)
            {
                try
                {
                    _ = await Client.PutAsJsonAsync(url, body);
                    break;
                }
                catch (HttpRequestException)
                {
                    continue;
                }
                catch
                {
                    throw;
                }
            }
        }

        private static async Task Seed()
        {
            FileInfo info = new(SeedFile);
            if (!info.Exists) return;

            using StreamReader reader = new(SeedFile);
            {
                string twitterUrl;
                while ((twitterUrl = reader.ReadLine()) != null)
                {
                    await Post(HomuraApiUrl, new Artist(twitterUrl));
                }
            }

            string seededFile = $"/data/seeded_{DateTimeOffset.Now.ToUnixTimeSeconds()}.txt";
            File.Move(SeedFile, seededFile);
        }

        private static async Task SendTelegramMessage(string message)
        {
            string id = Environment.GetEnvironmentVariable("TELEGRAM_BOT_ID");
            string token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            string chatId = Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID");
            
            string telegramUrl = $"https://api.telegram.org/bot{id}:{token}/sendMessage?chat_id={chatId}&text={message}";

            await Post(telegramUrl);
        }
    }
}