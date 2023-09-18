using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions.ShortUrlGenerator.Domain.Models
{
    public class ShortUrlEntity : TableEntity
    {
        public string Url { get; set; }
        public string Title { get; set; }

        public string ShortUrl { get; set; }

        public int Clicks { get; set; }

        public bool? IsActive { get; set; }

        public ShortUrlEntity() { }

        public ShortUrlEntity(string longUrl, string endUrl)
        {
            Initialize(longUrl, endUrl, string.Empty);
        }
        public ShortUrlEntity(string longUrl, string endUrl,string title)
        {
            Initialize(longUrl, endUrl, string.Empty);
        }

        private void Initialize(string longUrl, string endUrl, string title)
        {
            PartitionKey = endUrl.First().ToString();
            RowKey = endUrl;
            Url = longUrl;
            Title = title;
            Clicks = 0;
            IsActive = true;           
        }

        public static ShortUrlEntity GetEntity(string longUrl, string endUrl, string title)
        {
            return new ShortUrlEntity
            {
                PartitionKey = endUrl.First().ToString(),
                RowKey = endUrl,
                Url = longUrl,
                Title = title
            };
        }
    }
}
