using System.Collections.Generic;
using System.Threading.Tasks;
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

        [HttpGet]
        [Route("receiving/{name}")]
        public async Task<ReceivingPmode> GetReceiving(string name)
        {
            return await pmodeService.GetReceivingByName(name);
        }

        [HttpGet]
        [Route("sending")]
        public async Task<IEnumerable<string>> GetSendingPmodes()
        {
            return await pmodeService.GetSendingNames();
        }

        [HttpGet]
        [Route("sending/{name}")]
        public async Task<SendingPmode> GetSending(string name)
        {
            return await pmodeService.GetSendingByName(name);
        }
    }
}