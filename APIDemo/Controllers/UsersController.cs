using APIDemo.Model;
using APIDemo.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace APIDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IJwtService _jwtService;
        public UsersController(UserManager<IdentityUser> userManager, IJwtService jwtservice) // injection
        {
            _userManager = userManager;
            _jwtService = jwtservice;
        }

        [Route("AddUser")]
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user) // add user
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userManager.CreateAsync(
                new IdentityUser()
                {
                    UserName = user.UserName,
                    Email = user.Email
                },
                user.Password
            );

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            user.Password = null;
            return CreatedAtAction("GetUser", new { username = user.UserName }, user);
        }

        [HttpGet("GetUser/{username}")]
        public async Task<ActionResult<User>> GetUser(string username)
        {
            IdentityUser? user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return NotFound();
            }
            return new User
            {
                UserName = user.UserName,
                Email = user.Email
            };
        }

        // POST: api/Users/BearerToken
        [HttpPost("GetToken")]
        public async Task<ActionResult<AuthenticationResponse>> CreateBearerToken(AuthenticationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Bad credentials");
            }

            var user = await _userManager.FindByNameAsync(request.Username);

            if (user == null)
            {
                return BadRequest("User not found");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!isPasswordValid)
            {
                return BadRequest("Password does not match");
            }

            var token = _jwtService.CreateToken(user);

            return Ok(token);
        }
    }
}
