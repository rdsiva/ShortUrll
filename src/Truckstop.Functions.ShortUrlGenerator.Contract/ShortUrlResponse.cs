namespace Functions.ShortUrlGenerator.Contract
{
    public class ShortUrlResponse
    {
        public string ShortUrl { get; set; }
        public string LongUrl { get; set; }

        public ShortUrlResponse() { }
        public ShortUrlResponse(string host, string longUrl, string endUrl, string title)
        {
            LongUrl = longUrl;
            ShortUrl = string.Concat(host, "/", endUrl);

        }

    }
}