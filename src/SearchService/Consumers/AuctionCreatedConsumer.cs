using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

// MassTransit convention is the name this as Consumer
public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    
    private readonly IMapper _mapper;
    
    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine($"--> Consuming Auction created: {context.Message.Id}");
        
        var item = _mapper.Map<Item>(context.Message);

        await item.SaveAsync();
    }
}