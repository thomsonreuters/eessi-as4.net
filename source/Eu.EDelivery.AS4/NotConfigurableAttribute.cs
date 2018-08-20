using System;

namespace Eu.EDelivery.AS4
{
    /// <summary>
    /// Attribute used to hide a type from the ui
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class)]
    public class NotConfigurableAttribute : Attribute
    {
        
    }
}