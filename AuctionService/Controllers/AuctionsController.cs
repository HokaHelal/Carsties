﻿using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class AuctionsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly AuctionDbContext _context;

        public AuctionsController(AuctionDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
        {
            var auctions = await _context.Auctions.Include(x=>x.Item).OrderBy(x=>x.Item.Make).ToListAsync();

            return _mapper.Map<List<AuctionDto>>(auctions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            return _mapper.Map<AuctionDto>(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto dto)
        {
            var auction = _mapper.Map<Auction>(dto);
            // TODO: add current user as a seller
            auction.Seller = "test";
            _context.Auctions.Add(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return NotFound("Couldn't save changes in database");

            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));           
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AuctionDto>> UpdateAuction(Guid id, UpdateAuctionDto dto)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            auction.Item.Make = dto.Make ?? auction.Item.Make;
            auction.Item.Model = dto.Model ?? auction.Item.Model;
            auction.Item.Color = dto.Color ?? auction.Item.Color;
            auction.Item.Mileage = dto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = dto.Year ?? auction.Item.Year;

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest("Problem Saving Problem");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<AuctionDto>> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            _context.Auctions.Remove(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) BadRequest("Couldn't update DB");

            return Ok();
        }
    }
}
