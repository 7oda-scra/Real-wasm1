namespace RealDesign2.Client.Workspace;

public interface IWorkspaceTabService
{
    event Action? Changed;

    IReadOnlyList<WorkspaceTab> Tabs { get; }

    WorkspaceTab? ActiveTab { get; }

    int ActiveIndex { get; }

    WorkspaceTab OpenOrActivate(WorkspaceTabRequest request, bool updateRoute = true);

    WorkspaceTab OpenOrActivateRoute(string route, bool updateRoute = true);

    void Activate(string tabId, bool updateRoute = true);

    void ActivateIndex(int index, bool updateRoute = true);

    bool Close(string tabId);

    void Clear();

    void SetDirty(string tabId, bool isDirty);

    void UpdateTitle(string tabId, string title, string? subtitle = null);
}
