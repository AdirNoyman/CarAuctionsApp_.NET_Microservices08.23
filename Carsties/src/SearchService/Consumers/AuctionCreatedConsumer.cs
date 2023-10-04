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

            // Mocking fault for learning purposes
            if (item.Model == "Foo") throw new ArgumentException($"The model {item.Model} is not allowed");


            // Save the item to the search database
            await item.SaveAsync();

        }
    }
}