using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RealDesign2.Client.Interfaces;
using RealDesign2.Shared.Models;

namespace RealDesign2.Client.Services;

public sealed class ApiAuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthSessionStore _sessionStore;

    public ApiAuthService(HttpClient httpClient, IAuthSessionStore sessionStore)
    {
        _httpClient = httpClient;
        _sessionStore = sessionStore;
        ApplyAuthorizationHeader();
    }

    public event Action<UserDto?>? OnAuthStateChanged;

    public UserDto? CurrentUser => _sessionStore.CurrentUser;

    public async Task<(bool Success, UserDto? User)> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/auth/login",
            new LoginRequestDto
            {
                Username = username,
                Password = password
            },
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            ClearSession(notify: true);
            return (false, null);
        }

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The login response was empty.");

        _sessionStore.AccessToken = payload.AccessToken;
        _sessionStore.CurrentUser = payload.User;
        ApplyAuthorizationHeader();
        OnAuthStateChanged?.Invoke(payload.User);

        return (true, payload.User);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _httpClient.PostAsync("api/auth/logout", content: null, cancellationToken);
        }
        catch
        {
            // The client session is in-memory only, so we still clear local auth state even if the API is unavailable.
        }

        ClearSession(notify: true);
    }

    private void ClearSession(bool notify)
    {
        _sessionStore.AccessToken = null;
        _sessionStore.CurrentUser = null;
        ApplyAuthorizationHeader();

        if (notify)
        {
            OnAuthStateChanged?.Invoke(null);
        }
    }

    private void ApplyAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(_sessionStore.AccessToken)
            ? null
            : new AuthenticationHeaderValue("Bearer", _sessionStore.AccessToken);
    }
}
