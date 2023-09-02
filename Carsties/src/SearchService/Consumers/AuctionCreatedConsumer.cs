using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService.Consumers
{
    // Consume the event of auction created (by the auction service)
    public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
    {
        private readonly IMapper _mapper;

        public AuctionCreatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }

        // Consume the auction created event (created and published by the auction service)
        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            Console.WriteLine($"Consuming auction created -->  {context.Message.Id}");

            // Get the Item that was created, from the auction created event
            var item = _mapper.Map<Models.Item>(context.Message);

            // Save the item to the search database
            await item.SaveAsync();

        }
    }
}