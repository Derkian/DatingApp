using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);        
        Task<IEnumerable<AppUser>> GetUserAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByNameAsync(string username);

        Task<PagedList<MemeberDto>> GetMemebersAsync(UserParams userParams);

        Task<MemeberDto> GetMemeberAsync(string username, bool isCurrentUser);

        Task<string> GetUserGender(string username);

        Task<AppUser> GetUserByPhotoId(int photoId);
    }
}