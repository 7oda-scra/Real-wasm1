using System.Net.Http.Json;
using RealDesign2.Client.Interfaces;
using RealDesign2.Shared.Models;

namespace RealDesign2.Client.Services;

public sealed class ApiModuleService : IModuleService
{
    private readonly HttpClient _httpClient;

    public ApiModuleService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ModuleDefinitionDto>> GetAllowedModulesAsync(CancellationToken cancellationToken = default)
    {
        var modules = await _httpClient.GetFromJsonAsync<List<ModuleDefinitionDto>>("api/modules", cancellationToken);
        return modules ?? [];
    }

    public async Task<IReadOnlyList<ModuleDefinitionDto>> GetAllModulesAsync(CancellationToken cancellationToken = default)
    {
        var modules = await _httpClient.GetFromJsonAsync<List<ModuleDefinitionDto>>("api/modules/all", cancellationToken);
        return modules ?? [];
    }
}
