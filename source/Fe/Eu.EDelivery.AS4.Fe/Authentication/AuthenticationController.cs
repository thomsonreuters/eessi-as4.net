using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Eu.EDelivery.AS4.Fe.Authentication
{
    /// <summary>
    /// Authentication controller
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly ITokenService tokenService;
        private readonly UserManager<ApplicationUser> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
        /// </summary>
        /// <param name="tokenService">The token service.</param>
        /// <param name="userManager">The user manager.</param>
        public AuthenticationController(ITokenService tokenService, UserManager<ApplicationUser> userManager)
        {
            this.tokenService = tokenService;
            this.userManager = userManager;
        }

        /// <summary>
        /// Login using username / password combination
        /// </summary>
        /// <param name="login">The login payload</param>
        /// <returns>
        /// Json containing access token if login has succeeded
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(LoginSuccessModel), "Login was successful")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, typeof(UnauthorizedResult), "Login failed")]
        [ProducesResponseType(typeof(UnauthorizedResult), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            var user = await userManager.FindByNameAsync(login.Username);
            var result = await userManager.CheckPasswordAsync(user, login.Password);

            if (result)
                return new OkObjectResult(new LoginSuccessModel
                {
                    AccessToken = await tokenService.GenerateToken(user)
                });

            return new UnauthorizedResult();
        }

        /// <summary>
        /// Login using external provider
        /// </summary>
        /// <param name="provider">The name of the provider</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("externallogin")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        public async Task<IActionResult> ExternalLogin(string provider = null)
        {
            await HttpContext.ChallengeAsync(provider, new AuthenticationProperties { RedirectUri = "http://localhost:3000/#/login?callback=true" });
            return new OkResult();
        }

        /// <summary>
        /// Callback url used for external providers after login
        /// </summary>
        /// <param name="provider">The name of the provider</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("externallogincallback")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(LoginSuccessModel), "Login was successful")]
        public async Task<IActionResult> ExternalLoginCallback(string provider)
        {
            var isAuthenticated = await HttpContext.AuthenticateAsync(provider);
            if (isAuthenticated.Principal?.Identity?.IsAuthenticated != true) return new UnauthorizedResult();
            await HttpContext.SignOutAsync("Cookies");
            return new OkObjectResult(new LoginSuccessModel
            {
                AccessToken = await tokenService.GenerateToken(await userManager.GetUserAsync((ClaimsPrincipal)User.Identity))
            });
        }
    }
}