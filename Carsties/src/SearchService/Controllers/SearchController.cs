using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;
using ZstdSharp.Unsafe;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItem([FromQuery] SearchParams searchParams)
        {
            // Create the query object
            var query = DB.PagedSearch<Item, Item>();
            // Check if the search term is not null or empty
            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
            }

            // SORT the query reult set /////////////////////////////////////
            query = searchParams.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(a => a.Make)),
                // "new" = new auctions
                "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
                // Default sort will be by auction ending the soonest
                _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
            };

            // FILTER the query reult set /////////////////////////////////////
            query = searchParams.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Match(x => x.AuctionEnd > DateTime.UtcNow
                 && x.AuctionEnd < DateTime.UtcNow.AddHours(1)),
                // Default filter will be for auctions that are still active
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
            };

            // Make sure to bring back from the query, only documents that have a seller and winner value that were requested
            if (!string.IsNullOrEmpty(searchParams.Seller))
            {
                query.Match(x => x.Seller == searchParams.Seller);
            }

            if (!string.IsNullOrEmpty(searchParams.Winner))
            {
                query.Match(x => x.Winner == searchParams.Winner);
            }

            query.PageNumber(searchParams.PageNumber).PageSize(searchParams.PageSize);

            var result = await query.ExecuteAsync();

            return Ok(new
            {
                results = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount,
            });

        }
    }
}