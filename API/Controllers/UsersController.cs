using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        public IUserRepository UserRepository { get; }
        private readonly IMapper _mapper;
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _mapper = mapper;
            UserRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemeberDto>>> GetUsers()
        {
            return Ok(await UserRepository.GetMemebersAsync());
        }

        //api/users/1          
        [HttpGet("{username}")]
        public async Task<ActionResult<MemeberDto>> GetUser(string username)
        {
            return await UserRepository.GetMemeberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;            
            var user = await UserRepository.GetUserByNameAsync(username);

            _mapper.Map(memberUpdateDto, user);

            UserRepository.Update(user);

            if(await UserRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }
    }
}