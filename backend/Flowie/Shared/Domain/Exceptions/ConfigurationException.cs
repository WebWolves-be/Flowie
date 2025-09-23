namespace Flowie.Shared.Domain.Exceptions;

public class ConfigurationException : DomainException
{
    public string? ConfigurationKey { get; }

    public ConfigurationException() : base("Configuration error.")
    {
    }

    public ConfigurationException(string message) : base(message)
    {
    }

    public ConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ConfigurationException(string configurationKey, string message) 
        : base(message)
    {
        ConfigurationKey = configurationKey;
    }
}