using MudBlazor;
using RealDesign2.Client.Pages;

namespace RealDesign2.Client.Workspace;

public static class WorkspaceTabCatalog
{
    public static WorkspaceTabRequest Dashboard()
    {
        return new WorkspaceTabRequest
        {
            Id = "dashboard:home",
            Title = "Dashboard",
            Icon = Icons.Material.Filled.Home,
            Route = "/",
            ComponentType = typeof(Home),
            IsPinned = true
        };
    }

    public static WorkspaceTabRequest ForModule(string name, string route, string icon)
    {
        var normalizedRoute = NormalizeRoute(route);

        return normalizedRoute switch
        {
            "/" => Dashboard(),
            "/hr" => new WorkspaceTabRequest
            {
                Id = "module:hr",
                Title = "Human Resources",
                Icon = icon,
                Route = "/hr",
                ComponentType = typeof(Hr)
            },
            _ => new WorkspaceTabRequest
            {
                Id = $"module:{normalizedRoute.Trim('/').Replace('/', ':')}",
                Title = name,
                Icon = icon,
                Route = normalizedRoute,
                ComponentType = typeof(ModulePlaceholder),
                Parameters = new Dictionary<string, object?>
                {
                    ["Title"] = name,
                    ["Icon"] = icon
                }
            }
        };
    }

    public static WorkspaceTabRequest ForRoute(string route)
    {
        var normalizedRoute = NormalizeRoute(route);

        return normalizedRoute switch
        {
            "/" => Dashboard(),
            "/hr" => ForModule("Human Resources", "/hr", Icons.Material.Filled.Groups),
            "/pos" => ForModule("Point of Sale", "/pos", Icons.Material.Filled.PointOfSale),
            "/inventory" => ForModule("Inventory", "/inventory", Icons.Material.Filled.Inventory),
            "/sales" => ForModule("Sales", "/sales", Icons.Material.Filled.TrendingUp),
            "/purchases" => ForModule("Purchases", "/purchases", Icons.Material.Filled.ShoppingCart),
            "/crm" => ForModule("Customer Relationship Management", "/crm", Icons.Material.Filled.SupportAgent),
            "/exports-imports" => ForModule("Exports / Imports", "/exports-imports", Icons.Material.Filled.ImportExport),
            _ => new WorkspaceTabRequest
            {
                Id = $"route:{normalizedRoute.Trim('/').Replace('/', ':')}",
                Title = "Page not found",
                Subtitle = normalizedRoute,
                Icon = Icons.Material.Filled.WarningAmber,
                Route = normalizedRoute,
                ComponentType = typeof(WorkspaceNotFound)
            }
        };
    }

    public static WorkspaceTabRequest EmployeeDetails(string employeeId, string employeeName, string title, string email, string department)
    {
        return new WorkspaceTabRequest
        {
            Id = $"employee:{employeeId}",
            Title = $"Employee - {employeeName}",
            Subtitle = employeeId,
            Icon = Icons.Material.Filled.Badge,
            Route = $"/hr?employee={Uri.EscapeDataString(employeeId)}",
            ComponentType = typeof(EmployeeDetailsWorkspaceTab),
            Parameters = new Dictionary<string, object?>
            {
                ["EmployeeId"] = employeeId,
                ["EmployeeName"] = employeeName,
                ["Title"] = title,
                ["Email"] = email,
                ["Department"] = department
            }
        };
    }

    private static string NormalizeRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return "/";
        }

        var path = route.Split('?', '#')[0].Trim();
        if (path.Length == 0)
        {
            return "/";
        }

        return path.StartsWith('/') ? path : $"/{path}";
    }
}
