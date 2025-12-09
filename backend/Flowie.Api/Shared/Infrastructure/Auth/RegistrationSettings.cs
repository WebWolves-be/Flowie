namespace Flowie.Api.Shared.Infrastructure.Auth;

public class RegistrationSettings
{
    public const string SectionName = "Registration";

    public string Code { get; init; } = string.Empty;
}