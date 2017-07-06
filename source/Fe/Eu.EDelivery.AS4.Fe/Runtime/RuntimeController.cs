using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Eu.EDelivery.AS4.Fe.Runtime
{
    /// <summary>
    /// Controller to get AS4 runtime types
    /// </summary>
    [Route("api/[controller]")]
    public class RuntimeController
    {
        private readonly IRuntimeLoader runtimeLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeController"/> class.
        /// </summary>
        /// <param name="runtimeLoader">The runtime loader.</param>
        public RuntimeController(IRuntimeLoader runtimeLoader)
        {
            this.runtimeLoader = runtimeLoader;
        }

        /// <summary>
        /// Get receivers
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getreceivers")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        public IEnumerable<ItemType> GetReceivers()
        {
            return runtimeLoader.Receivers;
        }

        /// <summary>
        /// Get a list of steps.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getsteps")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        public IEnumerable<ItemType> GetSteps()
        {
            return runtimeLoader.Steps;
        }

        /// <summary>
        /// Gets the transformer list.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("gettransformers")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        public IEnumerable<ItemType> GetTransformerList()
        {
            return runtimeLoader.Transformers;
        }

        /// <summary>
        /// Gets the certificate repositories.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getcertificaterepositories")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        public IEnumerable<ItemType> GetCertificateRepositories()
        {
            return runtimeLoader.CertificateRepositories;
        }

        /// <summary>
        /// Gets the deliver senders.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getdeliversenders")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        public IEnumerable<ItemType> GetDeliverSenders()
        {
            return runtimeLoader.DeliverSenders;
        }

        /// <summary>
        /// Gets all runtime types.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getall")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        public IActionResult GetAllRuntimeTypes()
        {
            return new ObjectResult(new
            {
                Receivers = GetReceivers(),
                Steps = GetSteps(),
                Transformers = GetTransformerList(),
                CertificateRepositories = GetCertificateRepositories(),
                DeliverSenders = GetDeliverSenders(),
                RuntimeMetaData = JObject.Parse(JsonConvert.SerializeObject(runtimeLoader.ReceivingPmode, Formatting.Indented, new FlattenRuntimeToJsonConverter()))
            });
        }

        /// <summary>
        /// Get metadata for all the AS4 runtime types.
        /// This will return all the attributes used in the code.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getruntimemetadata")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        public IActionResult GetRuntimeMetaData()
        {
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(runtimeLoader.ReceivingPmode, Formatting.Indented, new FlattenRuntimeToJsonConverter()),
                ContentType = "application/json"
            };
        }
    }
}