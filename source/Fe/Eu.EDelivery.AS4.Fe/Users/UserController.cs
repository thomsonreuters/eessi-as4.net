using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Authentication;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Eu.EDelivery.AS4.Fe.Users
{
    /// <summary>
    ///     Controller to manage users
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Admin)]
    public class UserController : Controller
    {
        private readonly IUserService userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="userService">The user service.</param>
        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of users</returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(IEnumerable<User>))]
        [SwaggerResponse((int)HttpStatusCode.ExpectationFailed, typeof(ErrorModel), "Password requirements were not met or something else went wrong.")]
        public async Task<IActionResult> Get()
        {
            return new OkObjectResult(await userService.Get());
        }

        /// <summary>
        /// Creates the specified new user.
        /// </summary>
        /// <param name="newUser">The new user.</param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.ExpectationFailed, typeof(ErrorModel), "User already exists")]
        public async Task<IActionResult> Create([FromBody] NewUser newUser)
        {
            await userService.Create(newUser);
            return new OkResult();
        }

        /// <summary>
        /// Delete an existing user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{username}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.ExpectationFailed, typeof(ErrorModel), "User doesn't exist")]
        public async Task<IActionResult> Delete(string username)
        {
            await userService.Delete(username);
            return new OkResult();
        }

        /// <summary>
        /// Change a user password
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="update">The update.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{username}")]
        [SwaggerResponse((int) HttpStatusCode.OK)]
        [SwaggerResponse((int) HttpStatusCode.ExpectationFailed, typeof(ErrorModel), "User doesn't exist or password requirements were not met.")]
        public async Task<IActionResult> Update(string username, [FromBody]UpdateUser update)
        {
            await userService.Update(username, update);
            return new OkResult();
        }
    }
}
