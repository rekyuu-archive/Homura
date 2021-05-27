using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace HomuraApi.Models
{
    public class Artist
    {
        [Key] public long TwitterId { get; set; }
        [NotNull] public string TwitterUsername { get; set; }
        public long? LastProcessedTweetId { get; set; }
        [NotMapped] public long[] Media { get; set; }

        [NotMapped] public string TwitterUrl
        {
            set => _twitterUrl = value;
        }
        
        private string _twitterUrl;
        
        public async Task Initialize(TwitterClient client)
        {
            if (string.IsNullOrEmpty(TwitterUsername) && string.IsNullOrEmpty(_twitterUrl)) return;
            if (string.IsNullOrEmpty(TwitterUsername))
            {
                if (string.IsNullOrEmpty(_twitterUrl)) return;

                Regex rx = new(@"http(?:s)?:\/\/(?:www\.)?twitter\.com\/(?<username>[a-zA-Z0-9_]+)(?:\/.*)?", RegexOptions.IgnoreCase);
                MatchCollection matches = rx.Matches(_twitterUrl);

                if (matches.Count != 1) return;

                TwitterUsername = matches[0].Groups["username"].Value;
            }
            
            IUser user = await client.Users.GetUserAsync(TwitterUsername);
            
            TwitterId = user.Id;
            TwitterUsername = user.ScreenName;
            
            await GetMediaTweets(client);
            LastProcessedTweetId = Media.Last();
            Media = Array.Empty<long>();
        }

        public async Task GetMediaTweets(TwitterClient client)
        {
            GetUserTimelineParameters parameters = new(TwitterId)
            {
                IncludeRetweets = false,
                SinceId = LastProcessedTweetId
            };
            
            ITweet[] tweets = await client.Timelines.GetUserTimelineAsync(parameters);

            if (tweets.Length == 0) return;

            TwitterUsername = tweets[0].CreatedBy.ScreenName;
            Media = tweets
                .Where(x => x.Media.Count > 0)
                .Select(x => x.Id)
                .Reverse()
                .ToArray();
        }
    }
    
    public class ArtistContext : DbContext
    {
        public DbSet<Artist> Artists { get; set; }
        
        public ArtistContext(DbContextOptions<ArtistContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=/data/homura.db");
        }
    }
}