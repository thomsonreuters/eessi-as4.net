using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Eu.EDelivery.AS4.PayloadService.Infrastructure.SwaggerUtils
{
    /// <summary>
    /// <see cref="IOperationFilter"/> implementation to implement a 'File Upload' into Swagger.
    /// </summary>
    public class FileUploadOperation : IOperationFilter
    {
        /// <summary>
        /// Apply the 'File Upload' to the given <paramref name="operation"/>.
        /// </summary>
        /// <param name="operation">The Operation.</param>
        /// <param name="context">The Context.</param>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.OperationId.ToLower() == "apipayloaduploadpost")
            {
                if (operation.Parameters == null)
                {
                    operation.Parameters = new List<IParameter>();
                }
                
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "File",
                    In = "formData",
                    Description = "Upload Image",
                    Required = true,
                    Type = "file"
                });

                operation.Consumes.Add("application/form-data");
            }
        }
    }    
}
