using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;
[Route("api/[controller]")]
[ApiController]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery]SearchParams searchParams)
    {
        var query = DB.PagedSearch<Item, Item>();

        // search
        if(!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        // sorting
        query = searchParams.OrderBy switch
        {
            // by make
            "make" => query.Sort(i => i.Ascending(a => a.Make)),
            // by createdAt
            "new" => query.Sort(i => i.Descending(a => a.CreatedAt)),
            // default
            _ => query.Sort(i => i.Ascending(a => a.AuctionEnd))
        };

        // filtering
        query = searchParams.FilterBy switch
        {
            // by finished
            "finished" => query.Match(i => i.AuctionEnd < DateTime.UtcNow),
            // by ending soon by 6 hours
            "endingsoon" => query.Match(i => i.AuctionEnd > DateTime.UtcNow && i.AuctionEnd < DateTime.UtcNow.AddHours(6)),
            // default live action
            _ =>  query.Match(i => i.AuctionEnd > DateTime.UtcNow)
        };

        // filter by seller
        if(!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(i => i.Seller == searchParams.Seller);
        }

        // filter by winner
        if(!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(i => i.Winner == searchParams.Winner);
        }

        // paginate
        query.PageNumber(searchParams.PageNumber).PageSize(searchParams.PageSize);

        var result = await query.ExecuteAsync();

        return Ok(new
        {
            result = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
}
