namespace Kurrent.Models.Data.API;

public record DockerLoginResponse(string Token, string RefreshToken)
{
    public DateTimeOffset ExpiresOn => DateTimeOffset.FromUnixTimeSeconds(3600);
    public bool HasExpired => DateTimeOffset.UtcNow > ExpiresOn;
}