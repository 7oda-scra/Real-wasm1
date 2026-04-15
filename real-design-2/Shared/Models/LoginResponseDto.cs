namespace RealDesign2.Shared.Models;

public sealed class LoginResponseDto
{
    public required string AccessToken { get; set; }

    public required DateTimeOffset ExpiresAtUtc { get; set; }

    public required UserDto User { get; set; }
}
