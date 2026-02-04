namespace Shared.Services.BuildingBlocks.Exceptions;

public sealed class TransactionFailedException : Exception
{
    public TransactionFailedException(string message, Exception innerException = null)
        : base(message, innerException)
    {
    }
}