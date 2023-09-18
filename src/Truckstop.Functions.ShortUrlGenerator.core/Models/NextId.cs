using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truckstop.Functions.ShortUrlGenerator.Domain.Models
{
    public class NextId : TableEntity
    {
        public int Id { get; set; }
    }
}
