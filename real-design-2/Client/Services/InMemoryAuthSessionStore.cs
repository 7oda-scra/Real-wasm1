using RealDesign2.Shared.Models;

namespace RealDesign2.Client.Services;

public sealed class InMemoryAuthSessionStore : IAuthSessionStore
{
    public string? AccessToken { get; set; }

    public UserDto? CurrentUser { get; set; }
}
