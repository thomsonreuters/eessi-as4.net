using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Authentication;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    /// <summary>
    /// Controller to manage pmodes
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route("api/[controller]")]
    public class PmodeController : Controller
    {
        private readonly IPmodeService pmodeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PmodeController"/> class.
        /// </summary>
        /// <param name="pmodeService">The pmode service.</param>
        public PmodeController(IPmodeService pmodeService)
        {
            this.pmodeService = pmodeService;
        }

        /// <summary>
        /// Get a list of receiving pmode names.
        /// </summary>
        /// <returns>String list with all the pmode names.</returns>
        [HttpGet]
        [Route("receiving")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        public async Task<IEnumerable<string>> GetReceivingPmodes()
        {
            return await pmodeService.GetReceivingNames();
        }

        /// <summary>
        /// Create a receiving pmode
        /// </summary>
        /// <param name="basePmode">Pmode data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("receiving")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task CreateReceiving([FromBody] ReceivingBasePmode basePmode)
        {
            await pmodeService.CreateReceiving(basePmode);
        }

        /// <summary>
        /// Update existing receiving pmode
        /// </summary>
        /// <param name="basePmode">The base pmode.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("receiving/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task UpdateReceiving([FromBody] ReceivingBasePmode basePmode, string originalName)
        {
            await pmodeService.UpdateReceiving(basePmode, originalName);
        }

        /// <summary>
        /// Get a receiving pmode by name
        /// </summary>
        /// <param name="name">The name of the receiving pmode</param>
        /// <returns></returns>
        [HttpGet]
        [Route("receiving/{name}")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        public async Task<ReceivingBasePmode> GetReceiving(string name)
        {
            return await pmodeService.GetReceivingByName(name);
        }

        /// <summary>
        /// Delete an existing receiving pmode.
        /// </summary>
        /// <param name="name">The name of the pmode.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("receiving/{name}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task DeleteReceiving(string name)
        {
            await pmodeService.DeleteReceiving(name);
        }

        /// <summary>
        /// Get a list of sending pmode names
        /// </summary>
        /// <returns>String list of names</returns>
        [HttpGet]
        [Route("sending")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        public async Task<IEnumerable<string>> GetSendingPmodes()
        {
            return await pmodeService.GetSendingNames();
        }

        /// <summary>
        /// Create a sending pmode.
        /// </summary>
        /// <param name="basePmode">The pmode.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("sending")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task CreateSending([FromBody] SendingBasePmode basePmode)
        {
            await pmodeService.CreateSending(basePmode);
        }

        /// <summary>
        /// Get a sending pmode by name.
        /// </summary>
        /// <param name="name">The name of the pmode.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("sending/{name}")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        public async Task<SendingBasePmode> GetSending(string name)
        {
            return await pmodeService.GetSendingByName(name);
        }

        /// <summary>
        /// Delete an existing sending pmode.
        /// </summary>
        /// <param name="name">The name of the pmode.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("sending/{name}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task DeleteSending(string name)
        {
            await pmodeService.DeleteSending(name);
        }

        /// <summary>
        /// Update an existing pmode.
        /// </summary>
        /// <param name="basePmode">The pmode data.</param>
        /// <param name="originalPmodeName">Name of the original pmode.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("sending/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task UpdateSending([FromBody] SendingBasePmode basePmode, string originalName)
        {
            await pmodeService.UpdateSending(basePmode, originalName);
        }
    }
}