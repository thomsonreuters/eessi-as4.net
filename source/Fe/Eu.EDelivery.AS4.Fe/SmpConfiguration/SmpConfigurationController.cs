using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Authentication;
using Eu.EDelivery.AS4.Fe.SmpConfiguration.Model;
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
        [SwaggerResponse((int) HttpStatusCode.OK, typeof(IEnumerable<Model.SmpConfigurationRecord>))]
        public async Task<IEnumerable<SmpConfigurationRecord>> Get()
        {
            return await _smpConfiguration.GetAllData(
                smp => new SmpConfigurationRecord
                {
                    Id = smp.Id,
                    ToPartyId = smp.ToPartyId,
                    PartyType = smp.PartyType,
                    PartyRole = smp.PartyRole,
                    Url = smp.Url,
                    ServiceValue = smp.ServiceValue,
                    ServiceType = smp.ServiceType,
                    Action = smp.Action,
                    FinalRecipient = smp.FinalRecipient,
                    EncryptionEnabled = smp.EncryptionEnabled,
                    TlsEnabled = smp.TlsEnabled
                });
        }

        /// <summary>
        ///     Gets Smp configuration by identifier
        /// </summary>
        /// <param name="id">The identifier</param>
        /// <returns>Matching Smp configuration</returns>
        [HttpGet]
        [Route("{id}")]
        [SwaggerResponse((int) HttpStatusCode.OK, typeof(IEnumerable<SmpConfigurationDetail>))]
        public async Task<SmpConfigurationDetail> Get(int id)
        {
            return await _smpConfiguration.GetById(id);
        }

        /// <summary>
        ///     Posts the specified SMP configuration.
        /// </summary>
        /// <param name="smpConfiguration">The SMP configuration.</param>
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int) HttpStatusCode.OK, typeof(OkResult))]
        public async Task<IActionResult> Post([FromBody] SmpConfigurationDetail smpConfiguration)
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