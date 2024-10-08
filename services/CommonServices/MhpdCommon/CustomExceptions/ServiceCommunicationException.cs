namespace MhpdCommon.CustomExceptions;

public class ServiceCommunicationException : Exception
{
    public ServiceCommunicationException() { }

    public ServiceCommunicationException(string message) 
        : base(message) { }

    public ServiceCommunicationException(string message, Exception inner) 
        : base(message, inner) { }
}
