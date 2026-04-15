using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using RealDesign2.Client.Interfaces;
using RealDesign2.Shared.Models;

namespace RealDesign2.Client.Services;

public sealed class CustomAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly IAuthService _authService;
    private UserDto? _currentUser;

    public CustomAuthStateProvider(IAuthService authService)
    {
        _authService = authService;
        _currentUser = _authService.CurrentUser;
        _authService.OnAuthStateChanged += HandleAuthStateChanged;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(BuildAuthenticationState(_currentUser));
    }

    public void Dispose()
    {
        _authService.OnAuthStateChanged -= HandleAuthStateChanged;
    }

    private void HandleAuthStateChanged(UserDto? user)
    {
        _currentUser = user;
        NotifyAuthenticationStateChanged(Task.FromResult(BuildAuthenticationState(user)));
    }

    private static AuthenticationState BuildAuthenticationState(UserDto? user)
    {
        if (user is null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Username),
            new("FullName", user.FullName),
            new(ClaimTypes.Role, user.Role)
        };

        claims.AddRange(user.AllowedModuleIds.Select(moduleId => new Claim("AllowedModuleId", moduleId)));

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Bearer")));
    }
}
