# System Architecture

## Project Goal
A high-end, responsive Blazor Server/WebApp ERP system driven by `MudBlazor` UI components. The UI structure is entirely dynamic, driven by backend DTO definitions correlating `UserDto` access claims to available `ModuleDefinitionDto` modules.

## Data Layer & Services (Phase 1)
- **`Models/UserDto.cs`**: Encapsulates Identity (`Id`, `Username`, `FullName`, `Role`, `List<string> AllowedModuleIds`). 
- **`Models/ModuleDefinitionDto.cs`**: Abstracts ERP business domains (`Id`, `Name`, `IconPath`, `Route`, `Description`, `HexColor`).
- **`Interfaces` & `Services`**: 
  - `IAuthService` / `MockAuthService`: Exposes `<UserDto?> CurrentUser`, `<Task> LoginAsync`, `<Task> LogoutAsync`, and throws event `Action<UserDto?> OnAuthStateChanged`. Simulates 500ms network delay.
  - `IModuleService` / `MockModuleService`: Stores standard enterprise schemas (e.g. `pos`, `inventory`, `hr`). Relies on MudBlazor static Material Filled constants (`Icons.Material.Filled.*`) for `IconPath`.

## UI & Layout (Phase 2)
- **`MainLayout.razor`**: 
  - App Shell: `MudLayout` > `MudAppBar` (Elevation="1") & `MudDrawer` (Variant="DrawerVariant.Mini") providing a collapsible, YouTube-style left sidebar.
  - Theme: Instantiates `MudTheme` natively in C#. Identifiers: `Primary="#44abdf"`, `Background="#F9FAFB"`, `Surface="#FFFFFF"`. Uses `DefaultTypography` pointing to `Inter`, `Roboto`, `sans-serif`.
- **`NavMenu.razor`**: Interrogates the `IModuleService` injected via HTTP context. Generates interactive `MudNavLink` (Match="NavLinkMatch.All") nested in a `MudNavGroup` solely for authorized modules mapped to the user.

## Routing Security & State (Phase 3)
- **`CustomAuthStateProvider`**: Avoids standard circular dependency injection logic by subbing onto `IAuthService.OnAuthStateChanged`. Translates `UserDto` properties into standard `ClaimsPrincipal` including standard parsing of `ClaimTypes.Role`, scaling out `AllowedModuleId` generically into custom individual Claims.
- **`Routes.razor`**: Intercepts DOM routing wrapped with `<AuthorizeRouteView>`. Redirects invalid users by dropping them through `<RedirectToLogin>`.
- **`_Imports.razor`**: Asserts a globally enforced default-deny infrastructure by inheriting `@attribute [Authorize]`. All subsequent components enforce authentication implicitly. 
- **`Login.razor`**: Extends an `EmptyLayout.razor` to strip the `MudDrawer` away, forcing a full-screen vertical viewport. Overrides global restriction using `@attribute [AllowAnonymous]`. Contains a `MudCard` utilizing inline `MudProgressCircular` binding on simulated Task latency.
- **`Program.cs` Environment**: Explicitly hooks `builder.Services.AddAuthentication("MockScheme").AddCookie("MockScheme", options => { options.LoginPath = "/login"; })`, `app.UseAuthentication()`, and `app.UseAuthorization()`. The `LoginPath` override is required on .NET 9 SSR to prevent the default ASP.NET Core cookie challenge from redirecting to the non-existent `/Account/Login` path — pointing it instead to our Blazor `/login` route. `CustomAuthStateProvider` handles all actual auth state; `MockScheme` is purely a pipeline placeholder.

## Bento Dashboard (Phase 4)
- **`Home.razor`** (`@page "/"`, `@rendermode InteractiveServer`, `@attribute [Authorize]`): Authenticated landing page. Pulls `AuthenticationState` via injected `AuthenticationStateProvider`; extracts `FullName` from custom claim, falls back to `ClaimTypes.Name`. Reads all `AllowedModuleId` claims and calls `IModuleService.GetAllowedModulesAsync()` to populate a `List<ModuleDefinitionDto>`. Renders a time-of-day greeting header and a responsive `MudGrid` (xs=12, sm=6, md=4, lg=3) of bento cards. Empty-state `MudAlert` shown when no modules are assigned. `_isLoading` flag gates render with a centered `MudProgressCircular`.
- **`Home.razor.css`** (CSS isolation): Scoped `.bento-card` styles with `cubic-bezier` hover-lift transition, `translateY(-5px)` on hover with brand-border `#44abdf`, click-feedback `translateY(-2px)`, and `.icon-badge` circle (48×48px, `rgba(68,171,223,0.12)` background).
- **`Login.razor`**: Updated post-login redirect to `forceLoad: false` — allows Blazor client-side router to re-evaluate `AuthorizeRouteView` after `NotifyAuthenticationStateChanged` fires, without a full server round-trip.

## Phase 4.5: Auth Architecture & UI Polish
- **Global InteractiveServer rendering** (`App.razor`): `<Routes>` and `<HeadOutlet>` both set to `@rendermode="InteractiveServer"`, making the entire app a single persistent Blazor circuit. Page-level `@rendermode` directives removed as redundant.
- **`MockAuthService` promoted to Singleton** (`Program.cs`): Required so `CurrentUser` state persists across DI scope boundaries. Under `Scoped`, each new request/circuit created a fresh instance with `CurrentUser = null`, breaking the auth state after login. `MockModuleService` remains Scoped (stateless seed data).
- **`CustomAuthStateProvider` hardened**: Refactored claim-building into a private `BuildAuthenticationState(UserDto?)` method shared by both `GetAuthenticationStateAsync()` and `HandleAuthStateChanged()`. `NotifyAuthenticationStateChanged` now receives `Task.FromResult(newState)` — a pre-resolved Task — eliminating the async race window on the interactive circuit.
- **Bento card CSS moved to `app.css`** (global): Scoped CSS isolation (`.razor.css`) can fail to stamp `b-*` scope attributes when components render inside a globally-interactive router tree. Moving `.bento-card`, `.bento-card:hover`, `.bento-card:active`, and `.icon-badge` to `wwwroot/app.css` guarantees styles always apply.
- **Slate gray text** (`#6b7280`): `Color="Color.Secondary"` replaced with `Style="color: #6b7280;"` on date and module description text to eliminate MudBlazor's default pink secondary palette color.
## Phase 5: High-End Polish & Route Bridging
- **Login Form Upgrades**: `Login.razor` `<MudTextField>` controls wrapped in an `<EditForm Model="@_loginModel" OnValidSubmit="HandleLogin">` to bind the browser "Enter" key submit behavior automatically without writing DOM event handlers.
- **Top-Bar Sign Out Flow**: Configured a `MudMenu` on the `<MudAvatar>` dropdown in `MainLayout.razor` that fires `AuthService.LogoutAsync()`. Navigation resolves via `NavManager.NavigateTo("/login", forceLoad: true)` which forces a full HTTP refresh to drop the circuit and cleanly discard all state bounds.
- **Dangling Route Fixes**: Implemented exact mock `.razor` endpoint stubs (`/pos`, `/inventory`, `/hr`) resolving `NavMenu` 404 dead-ends.
- **Bento Dashboard Redesign (Home.razor)**: 
  - Greeting text re-mapped via `ClaimTypes.Name` correctly targeting "Username" over Identity "FullName".
## Phase 5.5: Architecture Stabilization
- **Avatar Menu Activation (`MainLayout.razor`)**: Corrected standard MudBlazor 7+ UI interaction by enforcing `ActivationEvent="@MouseEvent.LeftClick"` on the app-bar user `<MudAvatar>` inside an explicit `<ActivatorContent>` block. The MudMenuItem explicitly anchors the logout pipeline trigger natively.
- **Client-Side NavGroup Routing (`NavMenu.razor`)**: Hard browser 404 dead-ends were circumvented by anchoring relative paths dynamically via an interpolated explicit root identifier: `Href="@($"/{module.Route.TrimStart('/')}")"`.
- **Reusable `ModulePlaceholder.razor`**: Created unified, enterprise-grade `Under Construction` scaffolding. Deployed centralized component schema driving 7 module endpoints (`/pos`, `/inventory`, `/hr`, `/sales`, `/purchases`, `/crm`, `/exports-imports`). Binds seamlessly via `[Authorize]` parameter. Extraneous wording removed to strictly emphasize whitespace and minimalist aesthetics.

## Phase 6: HR Module Architecture
- **Master-Detail Layout (`Hr.razor`)**: Deployed responsive structural scaffolding utilizing `MudGrid`. The Master View is constrained to a static left column (`xs="12" md="3"`) utilizing a tiered `MudNavMenu` divided securely across logical domains (Employee Data, Shifts, Salary Settings). The Detail View populates a dynamic right staging column (`xs="12" md="9"`). Form visibility pivots on the local `_activeForm` state binding.
- **Sleek Form Interface Standard**: Set authoritative design matrix for internal data grids establishing `Elevation="0"`, muted borders (`1px solid #f0f0f0`), and uniform corner radius bindings (`rounded-xl`).
- **Standardized Control Headers**: Integrated unified action toolbars per form injecting inline FlexBox groupings (`d-flex align-center justify-space-between`). Standardized CRUD buttons (`Add`, `Modify`, `Delete`, `Save`) configured explicitly via pill bounds (`rounded-pill px-4`) prioritizing primary actionable coloring (`Color.Primary`) onto `Save`. Dismiss/close actions hooked to wipe `_activeForm` rendering the empty-state component dynamically.
- **Provider Assertions (`MainLayout.razor`)**: Guaranteed floating root dependency instantiation globally via explicit declarations of `<MudPopoverProvider />`, `<MudDialogProvider />`, and `<MudSnackbarProvider />` necessary for `MainLayout.razor` dropdown and modal DOM interception.

## Phase 6.5: Responsive Polish & Menu Fix
- **Avatar Menu Standardization**: Dispensed with custom event firing in favor of the canonical standard `<MudMenu ...>` to `<ActivatorContent>` pipeline to sidestep popover lifecycle conflicts entirely.
- **Drawer ClipMode**: Added `ClipMode="DrawerClipMode.Always"` to the App Shell `<MudDrawer>` ensuring it properly delegates viewport layering against the AppBar during mobile hamburger toggles.
- **Dynamic Mobile Deep-Linking (`Hr.razor`)**: Converted the static Master-Detail `MudGrid` into a viewport-aware layout component. Binding CSS toggles (`d-none d-md-block`) onto the columns linked against `_activeForm` means on mobile, the layout natively collapses the Master Menu out of view after a tap. Form view now explicitly checks for a mobile breakpoint bounding state natively returning a `<MudIconButton>` in the top toolbar to re-trigger the menu grid when needed.
- **Form Typography Polish**: Reduced the bounding box on dense configurations. Upgraded `<MudTextField>` and `<MudSelect>` across the HR prototype with `Margin="Margin.Dense"`. Introduced a structural `<MudDivider>` separator block beneath the toolbar actions for spatial distinction.
