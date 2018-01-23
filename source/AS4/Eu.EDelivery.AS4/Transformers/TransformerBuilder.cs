using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Transformers
{
    public class TransformerBuilder
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="TransformerBuilder"/> class from being created.
        /// </summary>
        private TransformerBuilder() { }

        /// <summary>
        /// Creates a <see cref="ITransformer"/> implementation based on the given <paramref name="config"/>.
        /// </summary>
        /// <param name="config">The configuration which contains the type and the optional settings for the <see cref="ITransformer"/> implementation.</param>
        /// <returns></returns>
        public static ITransformer FromTransformerconfig(Transformer config)
        {
            GenericTypeBuilder builder = GenericTypeBuilder.FromType(config.Type);

            if (config.Setting == null)
            {
                return builder.Build<ITransformer>();
            }

            var transformer = builder.Build<IConfigTransformer>();
            transformer.Configure(config.Setting.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase));

            return transformer;
        }
    }
}
