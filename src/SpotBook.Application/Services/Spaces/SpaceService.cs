using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpotBook.Application.DTOs.Request.Space;
using SpotBook.Application.DTOs.Response.Space;
using SpotBook.Application.Interfaces;
using SpotBook.Application.Interfaces.Spaces;

namespace SpotBook.Application.Services.Spaces
{
    public class SpaceService : ISpaceService
    {
        private readonly IAppDbContext _context;
        public SpaceService(IAppDbContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<SpaceResponse>> GetAllActiveSpacesAsync()
        {
            return await _context.Spaces
                    .AsNoTracking()
                    .Where(s => s.IsActive)
                    .Select(s => new SpaceResponse
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        Capacity = s.Capacity,
                        PricePerHour = s.PricePerHour,
                        IsActive = s.IsActive,
                        CreatedAt = s.CreatedAt
                    })
                    .ToListAsync();
        }

        public async Task<SpaceResponse?> GetSpaceByIdAsync(Guid id)
        {
            var space = await _context.Spaces
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == id);

            if (space == null) return null;

            return new SpaceResponse
            {
                Id = space.Id,
                Name = space.Name,
                Description = space.Description,
                Capacity = space.Capacity,
                PricePerHour = space.PricePerHour,
                IsActive = space.IsActive,
                CreatedAt = space.CreatedAt
            };
        }


        public async Task<SpaceResponse> CreateSpaceAsync(CreateSpaceRequest request)
        {
            var space = new Domain.Entities.Space
            {
                Name = request.Name,
                Description = request.Description,
                Capacity = request.Capacity,
                PricePerHour = request.PricePerHour,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Spaces.Add(space);
            await _context.SaveChangesAsync();
            return new SpaceResponse
            {
                Id = space.Id,
                Name = space.Name,
                Description = space.Description,
                Capacity = space.Capacity,
                PricePerHour = space.PricePerHour,
                IsActive = space.IsActive,
                CreatedAt = space.CreatedAt
            };
        }

        public async Task<SpaceResponse> UpdateSpaceAsync(Guid id, UpdateSpaceRequest request)
        {
            var space = await _context.Spaces.FirstOrDefaultAsync(s => s.Id == id);
            if (space == null)
            {
                throw new Exception("Ruangan tidak ditemukan.");
            }

            space.Name = request.Name;
            space.Description = request.Description;
            space.Capacity = request.Capacity;
            space.PricePerHour = request.PricePerHour;
            space.IsActive = request.IsActive;
            await _context.SaveChangesAsync();

            return new SpaceResponse
            {
                Id = space.Id,
                Name = space.Name,
                Description = space.Description,
                Capacity = space.Capacity,
                PricePerHour = space.PricePerHour,
                IsActive = space.IsActive,
                CreatedAt = space.CreatedAt
            };


        }
        public async Task<bool> DeleteSpaceAsync(Guid id)
        {
            var space = await _context.Spaces.FirstOrDefaultAsync(s => s.Id == id);
            if (space == null) return false;
            space.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }


}