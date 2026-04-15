using MudBlazor;
using RealDesign2.Shared.Models;

namespace RealDesign2.Server.Services;

public sealed class MockModuleCatalogService
{
    private readonly IReadOnlyList<ModuleDefinitionDto> _modules =
    [
        new ModuleDefinitionDto
        {
            Id = "pos",
            Name = "Point of Sale",
            IconPath = Icons.Material.Filled.PointOfSale,
            Route = "/pos",
            Description = "Manage retail transactions and sales.",
            HexColor = "#4CAF50"
        },
        new ModuleDefinitionDto
        {
            Id = "inventory",
            Name = "Inventory",
            IconPath = Icons.Material.Filled.Inventory,
            Route = "/inventory",
            Description = "Track stock, shipments, and warehouses.",
            HexColor = "#FF9800"
        },
        new ModuleDefinitionDto
        {
            Id = "hr",
            Name = "HR",
            IconPath = Icons.Material.Filled.People,
            Route = "/hr",
            Description = "Manage employees, payroll, and benefits.",
            HexColor = "#2196F3"
        },
        new ModuleDefinitionDto
        {
            Id = "sales",
            Name = "Sales",
            IconPath = Icons.Material.Filled.TrendingUp,
            Route = "/sales",
            Description = "Sales pipeline, orders, and forecasting.",
            HexColor = "#9C27B0"
        },
        new ModuleDefinitionDto
        {
            Id = "purchases",
            Name = "Purchases",
            IconPath = Icons.Material.Filled.ShoppingCart,
            Route = "/purchases",
            Description = "Manage vendors, purchase orders, and expenses.",
            HexColor = "#F44336"
        },
        new ModuleDefinitionDto
        {
            Id = "crm",
            Name = "CRM",
            IconPath = Icons.Material.Filled.SupportAgent,
            Route = "/crm",
            Description = "Customer relationship management and support.",
            HexColor = "#3F51B5"
        },
        new ModuleDefinitionDto
        {
            Id = "exports-imports",
            Name = "Exports/Imports",
            IconPath = Icons.Material.Filled.ImportExport,
            Route = "/exports-imports",
            Description = "Manage international trade and compliance.",
            HexColor = "#009688"
        }
    ];

    public IReadOnlyList<ModuleDefinitionDto> GetAllModules()
    {
        return _modules.Select(Clone).ToList();
    }

    public IReadOnlyList<ModuleDefinitionDto> GetAllowedModules(IEnumerable<string> allowedModuleIds)
    {
        var allowedSet = new HashSet<string>(allowedModuleIds, StringComparer.OrdinalIgnoreCase);

        return _modules
            .Where(module => allowedSet.Contains(module.Id))
            .Select(Clone)
            .ToList();
    }

    private static ModuleDefinitionDto Clone(ModuleDefinitionDto module)
    {
        return new ModuleDefinitionDto
        {
            Id = module.Id,
            Name = module.Name,
            IconPath = module.IconPath,
            Route = module.Route,
            Description = module.Description,
            HexColor = module.HexColor
        };
    }
}
