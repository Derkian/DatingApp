using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public PhotoRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Photo> GetPhotoById(int id)
        {
            return await _context.Photos                                 
                                 .IgnoreQueryFilters()
                                 .FirstOrDefaultAsync(p => p.Id == id);
            
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
        {
            return await _context.Photos
                        .IgnoreQueryFilters()
                        .Where(p => !p.isApproved)
                        .ProjectTo<PhotoForApprovalDto>(_mapper.ConfigurationProvider)                        
                        .ToListAsync();
                        
        }

        public void RemovePhoto(Photo photo)
        {
            _context.Photos.Remove(photo);
        }
    }
}