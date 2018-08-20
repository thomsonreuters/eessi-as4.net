using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Eu.EDelivery.AS4.Fe.Swagger
{
    /// <summary>
    /// Swagger operation filter to setup the submit tool upload data
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter" />
    public class FileUploadOperation : IOperationFilter
    {
        /// <summary>
        /// Applies the specified operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The context.</param>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.OperationId.ToLower() == "apisubmittoolpost")
            {
                operation.Consumes.Add("multipart/form-data");
                operation.Parameters = new List<IParameter>
                {
                    new NonBodyParameter
                    {
                        Name = "file",
                        In = "formData",
                        Description = "The payload to send with the message",
                        Required = false,
                        Type = "file"
                    },
                    new NonBodyParameter
                    {
                        Name = "pmode",
                        In = "formData",
                        Description = "The pmode to use to build the message",
                        Required = true,
                        Type = "string"
                    },
                    new NonBodyParameter
                    {
                        Name = "messages",
                        In = "formData",
                        Description = "The number of messages to send",
                        Required = true,
                        Default = 1,
                        Type = "number"
                    },
                    new NonBodyParameter
                    {
                        Name="payloadLocation",
                        In ="formData",
                        Description ="The location to send the payload to. Can be http:// or <c-d-e-...>",
                        Required = true,
                        Type = "string"
                    },
                    new NonBodyParameter
                    {
                        Name="to",
                        In="formData",
                        Description="The location to send the message to. Can be http:// or <c-d-e-...>:\\",
                        Required = true,
                        Type = "string"
                    }
                };
            }
        }
    }
}