using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts
{
    public class AuctionFinished
    {

        // Did the bid meet the reserve price and is the auction finished?
        public bool ItemSold { get; set; }
        public string AuctionId { get; set; }
        public string Winner { get; set; }
        public string Seller { get; set; }
        public int? Amount { get; set; }


    }
}