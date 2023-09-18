using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truckstop.Functions.ShortUrlGenerator.Contract
{
    public class ShortUrlClickStatsList
    {
        public List<ShortUrlClickStats> Items { get; set; }
        public string Url { get; set; }
        public ShortUrlClickStatsList()
        {
            Url = string.Empty;
        }
        public ShortUrlClickStatsList(List<ShortUrlClickStats> list)
        {
            Items = list;
            Url = string.Empty;
        }
    }
}
