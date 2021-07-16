using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        public UserManager<AppUser> UserManager { get; }
        private readonly IUnitOfWork _unitOfWork;
        public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            UserManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUserWithRoles()
        {
            var users = await UserManager.Users
                                .Include(r => r.UserRoles)
                                .ThenInclude(r => r.Role)
                                .OrderBy(u => u.UserName)
                                .Select(u => new
                                {
                                    u.Id,
                                    Username = u.UserName,
                                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                                }).ToListAsync();

            return Ok(users);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            var seleetedRoles = roles.Split(",").ToArray();

            var user = await UserManager.FindByNameAsync(username);

            if (user == null) return NotFound("Could not find user");

            var userRoles = await UserManager.GetRolesAsync(user);

            var result = await UserManager.AddToRolesAsync(user, seleetedRoles.Except(userRoles));

            if (!result.Succeeded) return BadRequest("Failed to add roles");

            result = await UserManager.RemoveFromRolesAsync(user, userRoles.Except(seleetedRoles));

            if (!result.Succeeded) return BadRequest("Failed to remove from roles");

            return Ok(await UserManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult> GetPhotosForModeration()
        {
            return Ok(await _unitOfWork.PhotoRepository.GetUnapprovedPhotos());
        }

        [HttpGet("photos-for-approval")]
        public async Task<ActionResult> GetPhotosForApproval()
        {
            return Ok(await _unitOfWork.PhotoRepository.GetUnapprovedPhotos());
        }

        [HttpPost("approve-photo/{photoId}")]
        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if(photo == null) return BadRequest("Could not find photo");

            photo.isApproved = true;

            var user = await _unitOfWork.UserRepository.GetUserByPhotoId(photoId);

            if(!user.Photos.Any(a => a.IsMain == true))
                photo.IsMain = true;

            if(await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to approve photo");
        }

        [HttpPost("reject-photo/{photoId}")]
        public async Task<ActionResult> RejectPhoto(int photoId)
        {
            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);

            _unitOfWork.PhotoRepository.RemovePhoto(photo);

            if(await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to approve photo");
        }
    }
}