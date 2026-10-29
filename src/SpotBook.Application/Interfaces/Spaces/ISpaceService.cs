using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotBook.Application.DTOs.Request.Space;
using SpotBook.Application.DTOs.Response.Space;

namespace SpotBook.Application.Interfaces.Spaces
{
    public interface ISpaceService
    {
        Task<IEnumerable<SpaceResponse>> GetAllActiveSpacesAsync();
        Task<SpaceResponse?> GetSpaceByIdAsync(Guid id);
        Task<SpaceResponse> CreateSpaceAsync(CreateSpaceRequest request);
        Task<SpaceResponse> UpdateSpaceAsync(Guid id, UpdateSpaceRequest request);
        Task<bool> DeleteSpaceAsync(Guid id);
        Task<IEnumerable<SpaceResponse>> GetAvailableSpacesAsync(DateTime startTime, DateTime endTime, int? minCapacity = null, decimal? maxPrice = null);
    }
}