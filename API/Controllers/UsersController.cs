using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        public IUserRepository UserRepository { get; }
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            UserRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemeberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var users = await UserRepository.GetMemebersAsync(userParams);

            Response.AddPaginationHeader(users.CurrentPage, 
                                        users.PageSize, 
                                        users.TotalCount, 
                                        users.TotalPages);

            return Ok(users);
        }

        //api/users/1          
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemeberDto>> GetUser(string username)
        {
            return await UserRepository.GetMemeberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            var user = await UserRepository.GetUserByNameAsync(User.GetUserName());

            _mapper.Map(memberUpdateDto, user);

            UserRepository.Update(user);

            if (await UserRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await UserRepository.GetUserByNameAsync(User.GetUserName());

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0)
                photo.IsMain = true;

            user.Photos.Add(photo);

            if (await UserRepository.SaveAllAsync())
                return CreatedAtRoute("GetUser", new { username = user.UserName} , _mapper.Map<PhotoDto>(photo));                

            return BadRequest("Problem adding photo");            
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await UserRepository.GetUserByNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(photo => photo.IsMain);
            if (currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;

            if (await UserRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo");
        }
        
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await UserRepository.GetUserByNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You cannot delete your main photo");

            if (photo.PublicId != null){
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);                
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await UserRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete photo");
        }
    }
}