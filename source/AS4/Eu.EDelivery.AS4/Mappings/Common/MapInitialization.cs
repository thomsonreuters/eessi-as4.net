using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Eu.EDelivery.AS4.Mappings.Submit;

namespace Eu.EDelivery.AS4.Mappings.Common
{
    public class MapInitialization
    {
        public static void InitializeMapper()
        {
            Mapper.Initialize(
                configurationExpression =>
                {
                    IEnumerable<Type> profiles = typeof(SubmitMessageMap).Assembly.GetTypes()
                        .Where(x => typeof(Profile).IsAssignableFrom(x));

                    foreach (Type profile in profiles)
                        configurationExpression.AddProfile(profile);
                });

            Mapper.Configuration.AssertConfigurationIsValid();
        }
    }
}