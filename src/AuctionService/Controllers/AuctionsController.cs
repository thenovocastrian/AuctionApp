using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAuctions(string date)
    {
        // Need to make this AsQueryable if we want to use this query to make other queries.
        var query = context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.UpdatedAt > DateTime.Parse(date).ToUniversalTime());
        }

        
        return await query.ProjectTo<AuctionDto>(mapper.ConfigurationProvider).ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }
        
        return mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = mapper.Map<Auction>(auctionDto);
        // TODO: Authentication - Add current user as seller
        auction.Seller = "Test";
        
        context.Auctions.Add(auction);
        
        var result = await context.SaveChangesAsync() > 0;
        
        var newAuction = mapper.Map<AuctionDto>(auction);
        
        await publishEndpoint.Publish(mapper.Map<AuctionCreated>(newAuction));
        
        if (!result) return BadRequest("Could not create auction in the database");
        
        return CreatedAtAction(nameof(GetAuctionById), 
            new {auction.Id }, newAuction);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        // Get the auction that matches the ID
        var auction = await context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();
        
        // TODO: Check seller == username
        
        // Wouldn't normally allow an auction to be updated after bids have been received
        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        var result = await context.SaveChangesAsync() > 0;
        
        if (!result) return BadRequest("Could not update auction in the database");
            
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        // Get the auction that matches the ID
        var auction = await context.Auctions.FindAsync(id);
        
        if (auction == null) return NotFound();
        
        // TODO: Check seller == username
        
        // Wouldn't normally allow an auction to be deleted after bids have been received
        context.Auctions.Remove(auction);
        
        var result = await context.SaveChangesAsync() > 0;
        
        if (!result)  return BadRequest("Could not delete auction in the database");
            
        return Ok();
    }
}