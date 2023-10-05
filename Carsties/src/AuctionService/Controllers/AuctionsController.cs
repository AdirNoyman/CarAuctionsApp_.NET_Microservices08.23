using Microsoft.AspNetCore.Mvc;
using AuctionService.Data;
using AutoMapper;
using AuctionService.DTOs;
using Microsoft.EntityFrameworkCore;
using AuctionService.Entities;
using AutoMapper.QueryableExtensions;
using MassTransit;
using Contracts;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        // Get all auctions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuctionDto>>> GetAllAuctions(string date)
        {
            // date = auction date
            // We use 'AsQueryable' to enables to make more than one query over this data we got from the db, before outputing the result
            var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

            Console.WriteLine("Date: " + date);

            if (!string.IsNullOrEmpty(date))
            {
                // Get all new auctions, meaning auctions that are created(or updated) after the date that was passed in the query string
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }

            // ProjectTo is a method from AutoMapper, that will map the query result to the AuctionDto. Afterwards we return the new auctions as list of auctions DTOs 
            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();

        }

        // Get individual auction
        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<AuctionDto>(auction));
        }

        // Create a new auction
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
        {
            // create a new instance of auction entity
            var auction = _mapper.Map<Auction>(createAuctionDto);
            // TODO: Add current session user, as the one who sells this auction item
            auction.Seller = "test";
            // Add the auction entity to the memory context 
            _context.Auctions.Add(auction);

            var newAuctionCreatedDto = _mapper.Map<AuctionDto>(auction);
            // Convert the new auction to the auction created contract and publish it to the broker service (rabbitMQ)
            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuctionCreatedDto));

            // Save the changes to the database, only after succsessful publish to the message broker. It will return one intger for each successful change
            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Failed to save changes to the DB");
            }

            // return the auction location
            return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, newAuctionCreatedDto);


        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null)
            {
                return NotFound();
            }

            // TODO: Check if the user is the seller of the auction, so we know if he is allowed to update this auction item

            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            // Convert the updated auction to the auction updated contract and publish it to the broker service (rabbitMQ)
            await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Failed to save changes to the DB");
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);

            if (auction == null) return NotFound();

            // TODO: Check if the user is the seller of the auction, so we know if he is allowed to delete this auction item

            _context.Auctions.Remove(auction);

            await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Failed to save changes to the DB");

            return Ok();


        }
    }
}