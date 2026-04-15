using RealDesign2.Shared.Models;

namespace RealDesign2.Client.Interfaces;

public interface IModuleService
{
    Task<IReadOnlyList<ModuleDefinitionDto>> GetAllowedModulesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ModuleDefinitionDto>> GetAllModulesAsync(CancellationToken cancellationToken = default);
}
