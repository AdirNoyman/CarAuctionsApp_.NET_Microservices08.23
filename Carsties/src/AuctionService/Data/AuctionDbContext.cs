using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data
{
    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions options) : base(options)
        {
            // the options are the database provider and the db connection string. They will be set in the Program.cs file
        }

        // Entites (tables) that are icluded in the app
        public DbSet<Auction> Auctions { get; set; }
    }
}