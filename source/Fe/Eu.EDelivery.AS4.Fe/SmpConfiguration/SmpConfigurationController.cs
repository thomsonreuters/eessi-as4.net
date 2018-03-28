using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Eu.EDelivery.AS4.Fe.SmpConfiguration
{
    /// <summary>
    ///     Smp configuration controller
    /// </summary>
    [Route("api/[controller]")]
    public class SmpConfigurationController
    {
        private readonly ISmpConfigurationService _smpConfiguration;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SmpConfigurationController" /> class.
        /// </summary>
        /// <param name="smpConfiguration">The SMP configuration.</param>
        public SmpConfigurationController(ISmpConfigurationService smpConfiguration)
        {
            _smpConfiguration = smpConfiguration;
        }

        /// <summary>
        ///     Get all Smp configurations
        /// </summary>
        /// <returns>List of SMP configurations</returns>
        [HttpGet]
        [SwaggerResponse((int) HttpStatusCode.OK, typeof(IEnumerable<Entities.SmpConfiguration>))]
        public async Task<IEnumerable<Entities.SmpConfiguration>> Get()
        {
            return await _smpConfiguration.GetAll();
        }

        /// <summary>
        ///     Posts the specified SMP configuration.
        /// </summary>
        /// <param name="smpConfiguration">The SMP configuration.</param>
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int) HttpStatusCode.OK, typeof(OkResult))]
        public async Task<IActionResult> Post([FromBody] SmpConfiguration smpConfiguration)
        {
            var configuration = await _smpConfiguration.Create(smpConfiguration);
            return new OkObjectResult(configuration);
        }

        /// <summary>
        ///     Puts the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="smpConfiguration">The SMP configuration.</param>
        [HttpPut]
        [Route("{id}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int) HttpStatusCode.OK)]
        public async Task<IActionResult> Put(int id, [FromBody] SmpConfiguration smpConfiguration)
        {         
            await _smpConfiguration.Update(id, smpConfiguration);
            return new OkResult();
        }

        /// <summary>
        ///     Delete an existing <see cref="Eu.EDelivery.AS4.Fe.SmpConfiguration" />
        /// </summary>
        /// <param name="id">The id of the <see cref="Eu.EDelivery.AS4.Fe.SmpConfiguration" /></param>
        [HttpDelete]
        [Route("{id}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int) HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(int id)
        {
            await _smpConfiguration.Delete(id);
            return new OkResult();
        }
    }
}