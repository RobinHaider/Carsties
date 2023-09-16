using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;
[Route("api/[controller]")]
[ApiController]
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

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAuctions(string date)
    {
        // query
        var query = _context.Auctions.OrderBy(x=>x.Item.Make).AsQueryable();

        // updated after date
        if(!string.IsNullOrEmpty(date))
        {
            var updatedAfter = DateTime.Parse(date).ToUniversalTime();
            query = query.Where(x=>x.UpdatedAt.CompareTo(updatedAfter) > 0);
        }

        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuction(Guid id)
    {
        var auction = await _context.Auctions
            .Include(x=>x.Item)
            .FirstOrDefaultAsync(x=>x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        return _mapper.Map<AuctionDto>(auction);
    }

    // create a new auction
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
    {
        var auction = _mapper.Map<Auction>(createAuctionDto);
        // TODO: current user as seller
        auction.Seller = "user";

        _context.Auctions.Add(auction);

        // new auctiondto
        var newAuction = _mapper.Map<AuctionDto>(auction);

        // auction created dto
        var auctionCreated = _mapper.Map<AuctionCreated>(newAuction);

        // publish to service bus
        await _publishEndpoint.Publish(auctionCreated);

        await _context.SaveChangesAsync();        

        return CreatedAtAction(nameof(GetAuction), new { id = auction.Id }, newAuction);
    }

    // update auction
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions
            .Include(x=>x.Item)
            .FirstOrDefaultAsync(x=>x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        _mapper.Map(updateAuctionDto, auction);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // delete auction
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);

        if (auction == null)
        {
            return NotFound();
        }

        _context.Auctions.Remove(auction);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
