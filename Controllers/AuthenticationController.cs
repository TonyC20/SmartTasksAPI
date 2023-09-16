using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SmartTasksAPI.DbContexts;
using SmartTasksAPI.Entities;
using SmartTasksAPI.Services;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace SmartTasksAPI.Controllers
{
    public class AuthenticationRequestBody
    {
        /// <summary>
        /// Account username
        /// </summary>
        public string Username { get; set; } = string.Empty;
        /// <summary>
        /// Password requires at least 1 digit and at least 5 characters
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }

    [Route("api/v{version:apiVersion}/account")]
    [ApiVersion("1.0")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IConfiguration _configuration;

        public AuthenticationController(UserManager<UserAccount> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Creates a user account from a username and password
        /// </summary>
        /// <param name="body"></param>
        /// <remarks>
        /// Pass in the desired username and password into the request body.<br/>
        /// Note: The password requires at least 1 digit and at least 5 characters. <br/><br/>
        /// Sample request:
        ///     
        ///     POST api/v1/account/create
        ///     {
        ///         "password": "customPassword1",
        ///         "username": "customUsername"
        ///     }
        /// 
        /// </remarks>
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Create(AuthenticationRequestBody body)
        {
            // Creates a new user
            var user = new UserAccount { UserName = body.Username };
            var result = await _userManager.CreateAsync(user, body.Password);
            // If account creation was successful
            if (result.Succeeded)
            {
                // Add some initial 
                return Ok();
            }

            return BadRequest(result.Errors);
        }


        /// <summary>
        /// Authenticates a user and returns a JWT security token if successful
        /// </summary>
        /// <param name="body">Pass in the username and password for the account in the body</param>
        /// <remarks>
        /// Note: The token expires after 120 minutes have passed.<br/>
        /// Sample request:
        ///
        ///     POST api/v1/account/authenticate
        ///     {
        ///         "password": "customPassword1",
        ///         "username": "customUsername"
        ///     }
        /// 
        /// </remarks>
        [HttpPost("authenticate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Authenticate(AuthenticationRequestBody body)
        {
            // Find the user by username
            var user = await _userManager.FindByNameAsync(body.Username);
            // If the user is found and the password is correct
            if (user != null && await _userManager.CheckPasswordAsync(user, body.Password))
            {
                // User authentication successful, create a new security token
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                // Create a symmetric security key from the secret key in the configuration
                var key = new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"]));
                // Create signing credentials using the symmetric security key and HMAC SHA256 encryption
                var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var jwtSecurityToken = new JwtSecurityToken(
                    _configuration["Authentication:Issuer"],
                    _configuration["Authentication:Audience"],
                    claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(Constants.JwtTokenValidTime),
                    signingCredentials: signingCredentials);
                var finalToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                // Return the token
                return Ok(new { token = finalToken });
            }
            // If the user is not found or the password is incorrect, return an Unauthorized result
            return Unauthorized("Invalid username or password");
        }
    }
}
