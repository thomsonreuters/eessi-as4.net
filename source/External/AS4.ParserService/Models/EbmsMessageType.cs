using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AS4.ParserService.Models
{
    public enum EbmsMessageType
    {
        Unknown = 0,
        UserMessage = 1,
        Receipt = 2,
        Error = 3
    }
}