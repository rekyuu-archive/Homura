using System;
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
        
        private static readonly HttpClient Client = new();

        private static async Task Main()
        {
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.Add("User-Agent", "HomuraWatcher");
            
            while (true)
            {
                try
                {
                    await Seed();
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

                    artist.LastProcessedTweetId = artist.Media.Last();
                    await Put(HomuraApiUrl, artist);
                }
                
                Thread.Sleep(MinutesBetweenPulls * 60 * 1000);
            }
        }

        private static async Task<T> Get<T>()
        {
            string response = await Client.GetStringAsync(HomuraApiUrl);
            return JsonConvert.DeserializeObject<T>(response);
        }

        private static async Task Post(string url, object body = null)
        {
            await Client.PostAsJsonAsync(url, body);
        }

        private static async Task Put(string url, object body)
        {
            await Client.PutAsJsonAsync(url, body);
        }

        private static async Task Seed()
        {
            await Post(HomuraApiUrl, new Artist("https://twitter.com/sanohito_mmd"));
            await Post(HomuraApiUrl, new Artist("https://twitter.com/Typpo8"));
            await Post(HomuraApiUrl, new Artist("https://twitter.com/NLO28636331"));
            await Post(HomuraApiUrl, new Artist("https://twitter.com/DirtyEro"));
            await Post(HomuraApiUrl, new Artist("https://twitter.com/SunsetSkyline1"));
            await Post(HomuraApiUrl, new Artist("https://twitter.com/squeezabledraws"));
            await Post(HomuraApiUrl, new Artist("https://twitter.com/bitibitixxxx"));
            await Post(HomuraApiUrl, new Artist("https://twitter.com/TostantanNSFW"));
            await Post(HomuraApiUrl, new Artist("https://twitter.com/yuka_nya_ff14"));
            await Post(HomuraApiUrl, new Artist("https://twitter.com/bigrbear"));
            await Post(HomuraApiUrl, new Artist("https://twitter.com/bartolomeobari2"));
            await Post(HomuraApiUrl, new Artist("https://twitter.com/ricegnat"));
            await Post(HomuraApiUrl, new Artist("https://twitter.com/Waero_Re"));
            await Post(HomuraApiUrl, new Artist("https://twitter.com/ydh2101_18"));
            await Post(HomuraApiUrl, new Artist("https://twitter.com/neko__nsfw"));
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