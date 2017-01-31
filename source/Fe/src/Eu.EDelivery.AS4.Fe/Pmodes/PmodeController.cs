using System.Collections.Generic;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Authentication;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    [Route("api/[controller]")]
    public class PmodeController : Controller
    {
        private readonly IAs4PmodeService pmodeService;

        public PmodeController(IAs4PmodeService pmodeService)
        {
            this.pmodeService = pmodeService;
        }

        [HttpGet]
        [Route("receiving")]
        public async Task<IEnumerable<string>> GetReceivingPmodes()
        {
            return await pmodeService.GetReceivingNames();
        }

        [HttpPost]
        [Route("receiving")]
        [Authorize(Roles = Roles.Admin)]
        public async Task CreateReceiving([FromBody] ReceivingPmode pmode)
        {
            await pmodeService.CreateReceiving(pmode);
        }

        [HttpPut]
        [Route("receiving/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task UpdateReceiving([FromBody] ReceivingPmode pmode, string originalName)
        {
            await pmodeService.UpdateReceiving(pmode, originalName);
        }

        [HttpGet]
        [Route("receiving/{name}")]
        public async Task<ReceivingPmode> GetReceiving(string name)
        {
            return await pmodeService.GetReceivingByName(name);
        }

        [HttpDelete]
        [Route("receiving/{name}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task DeleteReceiving(string name)
        {
            await pmodeService.DeleteReceiving(name);
        }

        [HttpGet]
        [Route("sending")]
        public async Task<IEnumerable<string>> GetSendingPmodes()
        {
            return await pmodeService.GetSendingNames();
        }

        [HttpPost]
        [Route("sending")]
        [Authorize(Roles = Roles.Admin)]
        public async Task CreateSending([FromBody] SendingPmode pmode)
        {
            await pmodeService.CreateSending(pmode);
        }

        [HttpGet]
        [Route("sending/{name}")]
        public async Task<SendingPmode> GetSending(string name)
        {
            return await pmodeService.GetSendingByName(name);
        }

        [HttpDelete]
        [Route("sending/{name}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task DeleteSending(string name)
        {
            await pmodeService.DeleteSending(name);
        }

        [HttpPut]
        [Route("sending/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task UpdateSending([FromBody] SendingPmode pmode, string originalName)
        {
            await pmodeService.UpdateSending(pmode, originalName);
        }
    }
}