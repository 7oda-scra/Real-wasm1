using RealDesign2.Shared.Models;

namespace RealDesign2.Client.Interfaces;

public interface IAuthService
{
    UserDto? CurrentUser { get; }

    event Action<UserDto?>? OnAuthStateChanged;

    Task<(bool Success, UserDto? User)> LoginAsync(string username, string password, CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);
}
