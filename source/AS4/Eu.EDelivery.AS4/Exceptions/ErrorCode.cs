namespace Eu.EDelivery.AS4.Exceptions
{
    /// <summary>
    /// AS4 Error Codes
    /// </summary>
    public enum ErrorCode
    {
        NotApplicable = 0,

        // ebMS Processing Errors
        Ebms0001 = 1,
        Ebms0002 = 2,
        Ebms0003 = 3,
        Ebms0004 = 4,
        Ebms0005 = 5,
        Ebms0006 = 6,
        Ebms0007 = 7,
        Ebms0008 = 8,
        Ebms0009 = 9,
        Ebms0010 = 10,
        Ebms0011 = 11,

        // Security Processing Errors
        Ebms0101 = 101,
        Ebms0102 = 102,
        Ebms0103 = 103,
        
        // Reliable Messaging Errors
        Ebms0201 = 201,
        Ebms0202 = 202,

        // Additional Features Errors
        Ebms0301 = 301,
        Ebms0302 = 302, 
        Ebms0303 = 303,
    }
}
