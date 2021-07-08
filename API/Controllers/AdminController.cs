using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        public UserManager<AppUser> UserManager { get; }
        public AdminController(UserManager<AppUser> userManager)
        {
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
                                .Select( u => new 
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

            if(user == null) return NotFound("Could not find user");

            var userRoles = await UserManager.GetRolesAsync(user);

            var result = await UserManager.AddToRolesAsync(user, seleetedRoles.Except(userRoles));

            if(!result.Succeeded) return BadRequest("Failed to add roles");

            result = await UserManager.RemoveFromRolesAsync(user, userRoles.Except(seleetedRoles));

            if(!result.Succeeded) return BadRequest("Failed to remove from roles");

            return Ok(await UserManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Only moderator can see it");
        }
    }
}