using System;

namespace Eu.EDelivery.AS4.Fe
{
  public class BusinessException : Exception
  {
    public BusinessException(string message) : base(message)
    {

    }
  }
}
