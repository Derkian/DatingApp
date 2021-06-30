using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<MemeberDto> GetMemeberAsync(string username)
        {
            return await _context.Users
                            .Where(x => x.UserName == username)
                            .ProjectTo<MemeberDto>(_mapper.ConfigurationProvider)
                            .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemeberDto>> GetMemebersAsync(UserParams userParams)
        {
            var query = _context.Users
                            .ProjectTo<MemeberDto>(_mapper.ConfigurationProvider)
                            .AsNoTracking();

            return await PagedList<MemeberDto>.CreateAsync(query, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<IEnumerable<AppUser>> GetUserAsync()
        {
            return await _context.Users
                    .Include(photo => photo.Photos)
                    .ToListAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users
                            .FindAsync(id);
        }

        public async Task<AppUser> GetUserByNameAsync(string username)
        {
            return await _context.Users
                            .Include(photo => photo.Photos)
                            .SingleOrDefaultAsync(user => user.UserName == username);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}