namespace SportsApi.domain.Abstractions.Exceptions;

public sealed class DomainConflictException(string message, string errorKey) : Exception(message)
{
    public string ErrorKey { get; } = errorKey;
}