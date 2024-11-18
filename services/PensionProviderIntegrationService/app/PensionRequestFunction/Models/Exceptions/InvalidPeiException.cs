namespace PensionRequestFunction.Models.Exceptions;

public class InvalidPeiException : Exception
{
    public InvalidPeiException()
    {
    }

    public InvalidPeiException(string? message) : base(message)
    {
    }

    public InvalidPeiException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
