using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // Mapping Entity to DTO
        CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item); // Maps Auction linked on Item
        CreateMap<Item, AuctionDto>();
        
        // Mapping DTO back to Entity
        CreateMap<CreateAuctionDto, Auction>()
            .ForMember(dest => dest.Item, 
                opt => opt.MapFrom(src => src)); // Use CreateAuctionDto for both Auction and its Item

        CreateMap<CreateAuctionDto, Item>();

        CreateMap<AuctionDto, AuctionCreated>();
    }
}