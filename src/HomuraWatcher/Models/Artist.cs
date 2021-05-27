namespace HomuraWatcher.Models
{
    public class Artist
    {
        public long TwitterId { get; set; }
        public string TwitterUsername { get; set; }
        public long? LastProcessedTweetId { get; set; }
        public long[] Media { get; set; }
        public string TwitterUrl { get; set; }

        public Artist(string url)
        {
            TwitterUrl = url;
        }
    }
}