using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Entities;
using MassTransit;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add this 3 entities(tables) to the model builder for creating the functionality of the outbox pattern dedicated for the broker service (rabbitMQ)
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

        }
    }
}