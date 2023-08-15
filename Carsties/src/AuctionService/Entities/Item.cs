using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionService.Entities
{
    [Table("Items")]
    public class Item
    {
        public  Guid Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public int Mileage { get; set; }
        public string ImageUrl { get; set; }

        // nav properties -> 1:1 relationship to between Auction and Item objects
        public Auction Auction  { get; set; }
        public Guid AuctionId { get; set; }
    }
}