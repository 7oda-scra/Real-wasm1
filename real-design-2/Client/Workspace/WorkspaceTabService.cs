using Microsoft.AspNetCore.Components;

namespace RealDesign2.Client.Workspace;

public sealed class WorkspaceTabService : IWorkspaceTabService
{
    private readonly NavigationManager _navigationManager;
    private readonly List<WorkspaceTab> _tabs = [];
    private string? _activeTabId;

    public WorkspaceTabService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public event Action? Changed;

    public IReadOnlyList<WorkspaceTab> Tabs => _tabs;

    public WorkspaceTab? ActiveTab => _tabs.FirstOrDefault(tab => tab.Id == _activeTabId);

    public int ActiveIndex => ActiveTab is { } activeTab ? _tabs.IndexOf(activeTab) : -1;

    public WorkspaceTab OpenOrActivate(WorkspaceTabRequest request, bool updateRoute = true)
    {
        var existing = _tabs.FirstOrDefault(tab => tab.Id == request.Id);

        if (existing is null)
        {
            existing = new WorkspaceTab
            {
                Id = request.Id,
                Title = request.Title,
                Subtitle = request.Subtitle,
                Icon = request.Icon,
                Route = request.Route,
                ComponentType = request.ComponentType,
                Parameters = request.Parameters,
                IsPinned = request.IsPinned
            };

            _tabs.Add(existing);
        }
        else
        {
            existing.LastActivatedAt = DateTimeOffset.UtcNow;
        }

        _activeTabId = existing.Id;
        NavigateToTabRoute(existing, updateRoute);
        NotifyChanged();
        return existing;
    }

    public WorkspaceTab OpenOrActivateRoute(string route, bool updateRoute = true)
    {
        return OpenOrActivate(WorkspaceTabCatalog.ForRoute(route), updateRoute);
    }

    public void Activate(string tabId, bool updateRoute = true)
    {
        var tab = _tabs.FirstOrDefault(item => item.Id == tabId);
        if (tab is null)
        {
            return;
        }

        _activeTabId = tab.Id;
        tab.LastActivatedAt = DateTimeOffset.UtcNow;
        NavigateToTabRoute(tab, updateRoute);
        NotifyChanged();
    }

    public void ActivateIndex(int index, bool updateRoute = true)
    {
        if (index < 0 || index >= _tabs.Count)
        {
            return;
        }

        Activate(_tabs[index].Id, updateRoute);
    }

    public bool Close(string tabId)
    {
        var tab = _tabs.FirstOrDefault(item => item.Id == tabId);
        if (tab is null || tab.IsPinned)
        {
            return false;
        }

        var removedIndex = _tabs.IndexOf(tab);
        _tabs.RemoveAt(removedIndex);

        if (_activeTabId == tabId)
        {
            var nextIndex = Math.Clamp(removedIndex - 1, 0, _tabs.Count - 1);
            _activeTabId = _tabs.Count > 0 ? _tabs[nextIndex].Id : null;
            NavigateToTabRoute(ActiveTab, updateRoute: true);
        }

        NotifyChanged();
        return true;
    }

    public void Clear()
    {
        _tabs.Clear();
        _activeTabId = null;
        NotifyChanged();
    }

    public void SetDirty(string tabId, bool isDirty)
    {
        var tab = _tabs.FirstOrDefault(item => item.Id == tabId);
        if (tab is null || tab.IsDirty == isDirty)
        {
            return;
        }

        tab.IsDirty = isDirty;
        NotifyChanged();
    }

    public void UpdateTitle(string tabId, string title, string? subtitle = null)
    {
        var tab = _tabs.FirstOrDefault(item => item.Id == tabId);
        if (tab is null)
        {
            return;
        }

        tab.Title = title;
        tab.Subtitle = subtitle;
        NotifyChanged();
    }

    private void NavigateToTabRoute(WorkspaceTab? tab, bool updateRoute)
    {
        if (!updateRoute || string.IsNullOrWhiteSpace(tab?.Route))
        {
            return;
        }

        var currentPath = "/" + _navigationManager.ToBaseRelativePath(_navigationManager.Uri).Split('?', '#')[0].Trim('/');
        if (currentPath == "//")
        {
            currentPath = "/";
        }

        var targetPath = tab.Route.Split('?', '#')[0];
        if (!string.Equals(currentPath, targetPath, StringComparison.OrdinalIgnoreCase))
        {
            _navigationManager.NavigateTo(tab.Route, forceLoad: false, replace: false);
        }
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
