using RealDesign2.Shared.Models;

namespace RealDesign2.Client.Services;

public interface IAuthSessionStore
{
    string? AccessToken { get; set; }

    UserDto? CurrentUser { get; set; }
}
