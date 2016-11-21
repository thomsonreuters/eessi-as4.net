using System;
using System.Threading.Tasks;
using AutoMapper;
using Eu.EDelivery.AS4.Fe.AS4Model;
using Eu.EDelivery.AS4.Fe.Logging;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Fe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eu.EDelivery.AS4.Fe.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ConfigurationController : Controller
    {
        private readonly IMapper mapper;
        private readonly IAs4SettingsService settingsService;

        public ConfigurationController(IAs4SettingsService settingsService, IMapper mapper, ILogging logging)
        {
            
            this.settingsService = settingsService;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<AS4Model.Settings> Get()
        {
            return await settingsService.GetSettings();
        }

        [HttpPost]
        [Route("basesettings")]
        public async Task<OkResult> SaveBaseSettings([FromBody] BaseSettings settings)
        {
            await settingsService.SaveBaseSettings(settings);
            return Ok();
        }

        [HttpPost]
        [Route("customsettings")]
        public async Task<OkResult> SaveCustomSettings([FromBody] CustomSettings settings)
        {
            await settingsService.SaveCustomSettings(settings);
            return Ok();
        }

        [HttpPost]
        [Route("databasesettings")]
        public async Task<OkResult> SaveDatabaseSettings([FromBody] SettingsDatabase settings)
        {
            await settingsService.SaveDatabaseSettings(settings);
            return Ok();
        }

        [HttpGet]
        [Route("test")]
        public void Test()
        {
            throw new Exception("test");
        }
    }
}