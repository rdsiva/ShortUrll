using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions.ShortUrlGenerator.Domain.Models
{
    public class ClickStatsEntity : TableEntity
    {
        //public string Id { get; set; }
        public string Datetime { get; set; }

        public ClickStatsEntity() { }

        public ClickStatsEntity(string vanity)
        {
            PartitionKey = vanity;
            RowKey = Guid.NewGuid().ToString();
            Datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
