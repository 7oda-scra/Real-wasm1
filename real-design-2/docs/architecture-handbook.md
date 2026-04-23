# Architecture Handbook

## Table of Contents
- [Phase 1: High-Level Architecture & Core Concept](#phase-1-high-level-architecture--core-concept)
- [Phase 2: The Core Most Important Files](#phase-2-the-core-most-important-files)
- [Phase 3: File-by-File Breakdown & Interactions](#phase-3-file-by-file-breakdown--interactions)
- [Phase 4: Centralization & Global Theming Guide](#phase-4-centralization--global-theming-guide)

## Phase 1: High-Level Architecture & Core Concept

### Executive Summary
`real-design-2.sln` is a hosted Blazor WebAssembly ERP shell built around MudBlazor, mock business data, and JWT-based route protection. The solution is intentionally split into three projects:

- `Client`: the Blazor WebAssembly front end, all layouts, pages, shared UI primitives, auth state translation, and client-side service abstractions.
- `Server`: the ASP.NET Core host and API surface. It serves the WASM assets, validates JWTs, issues mock JWTs, and exposes module/auth endpoints.
- `Shared`: the DTO contract layer used by both Client and Server to keep the HTTP surface strongly typed.

The result is a browser-executed UI that talks to a colocated backend over HTTP, but still preserves a clean contract boundary through shared models.

### Hosted Blazor WebAssembly Architecture

#### Client Project
The `Client` project boots through `WebAssemblyHostBuilder` and renders the application entirely in the browser. It owns:

- routing and authorization UX
- layouts and shared design primitives
- page-level module composition
- HTTP clients for auth and module retrieval
- translation of server-issued `UserDto` data into a `ClaimsPrincipal`

This is the only project that renders Razor components.

#### Server Project
The `Server` project is a traditional ASP.NET Core application that does three jobs:

- hosts the static WASM application via `UseBlazorFrameworkFiles()` and `MapFallbackToFile("index.html")`
- validates incoming JWT Bearer tokens
- exposes JSON endpoints for login, current user introspection, and module access

It does not render the UI. It is strictly the API and hosting boundary.

#### Shared Project
The `Shared` project contains DTOs and simple HR data models only. It is the contract seam between browser and API:

- auth request/response payloads
- module metadata payloads
- user identity payloads
- HR demo DTOs for the dynamic CRUD screens

This keeps both Client and Server compiling against the same shapes without duplicating models.

### In-Memory Mock JWT Authentication Flow
The authentication system is token-based, but intentionally lightweight and mock-backed:

1. The user submits credentials on `Client/Pages/Login.razor`.
2. `ApiAuthService` posts `LoginRequestDto` to `Server/Controllers/AuthController.cs`.
3. `AuthController` asks `MockUserStore` to validate the username/password.
4. If valid, `JwtTokenService` builds a signed JWT containing:
   - `NameIdentifier`
   - `Name`
   - `FullName`
   - `Role`
   - one `AllowedModuleId` claim per authorized module
5. The server returns `LoginResponseDto` with:
   - `AccessToken`
   - `ExpiresAtUtc`
   - `UserDto`
6. `ApiAuthService` stores the token and `UserDto` in `InMemoryAuthSessionStore`.
7. `ApiAuthService` sets the `Authorization: Bearer ...` header on the shared `HttpClient`.
8. `ApiAuthService` raises `OnAuthStateChanged`.
9. `CustomAuthStateProvider` listens for that event and rebuilds the Blazor `AuthenticationState`.
10. `App.razor` and `AuthorizeRouteView` re-evaluate authorization and unlock protected routes.
11. `NavMenu.razor` and `Home.razor` call `ApiModuleService`, which hits `api/modules`.
12. `ModulesController` reads the JWT claims and returns only the allowed modules.

Important operational detail: auth persistence is memory-only. A full browser refresh clears the token because `InMemoryAuthSessionStore` does not write to `localStorage`, cookies, or IndexedDB. This is a deliberate tradeoff in the current mock architecture.

## Phase 2: The Core Most Important Files

### 1. `Client/Program.cs`

#### Why it is critical
This is the composition root of the browser app. It builds the WASM host, configures the API base address, and registers every essential client-side service:

- MudBlazor
- authorization services
- auth session storage
- auth and module API abstractions
- the custom auth state provider
- the HR mock data service

If dependency injection is wrong here, the application cannot authenticate, route, or render properly.

#### The Blast Radius
A bug here can break the entire front end:

- wrong `HttpClient.BaseAddress` breaks every API call
- missing `AuthenticationStateProvider` registration breaks protected routing
- missing `IAuthService` or `IModuleService` registration breaks login and nav menu loading
- wrong root component registration prevents the app from booting

### 2. `Server/Program.cs`

#### Why it is critical
This is the backend composition root. It wires JWT validation, controller discovery, DI, static file hosting, and fallback routing for the hosted WASM model.

#### The Blast Radius
A bug here can take down the whole distributed system:

- JWT misconfiguration makes all authorized API calls fail
- missing `UseBlazorFrameworkFiles()` breaks the client shell
- missing `MapControllers()` makes auth and module APIs unreachable
- wrong auth pipeline ordering causes silent 401/403 failures

### 3. `Client/Services/ApiAuthService.cs`

#### Why it is critical
This file is the operational bridge between the login form and every secured API call. It owns:

- login POST submission
- token/header application
- session clearing
- auth-state event dispatch

It is the single place where raw server auth responses become client session state.

#### The Blast Radius
A bug here causes cascading security and usability failures:

- login may succeed server-side but never update the client session
- the bearer token may never be attached to future requests
- logout may leave stale authorization headers behind
- auth listeners may never be notified, leaving the UI visually out of sync

### 4. `Client/Services/CustomAuthStateProvider.cs`

#### Why it is critical
This file converts `UserDto` into the `ClaimsPrincipal` used by Blazor authorization. It is the reason `[Authorize]`, `AuthorizeRouteView`, `AuthorizeView`, and route protection work at all in the client.

#### The Blast Radius
A bug here creates subtle and severe authorization inconsistencies:

- authenticated users may appear anonymous
- roles may stop working
- allowed-module claims may disappear
- the UI may render routes the API later rejects

### 5. `Server/Controllers/AuthController.cs`

#### Why it is critical
This controller is the login gateway into the system. It validates credentials against the mock store and returns the JWT payload the entire client auth model depends on.

#### The Blast Radius
A bug here blocks all access:

- login becomes impossible
- the current-user endpoint becomes unreliable
- bad error handling produces broken UX on the sign-in screen
- malformed responses can poison the client auth session

## Phase 3: File-by-File Breakdown & Interactions

This section walks the solution by layer rather than alphabetically. “Interactions” focuses on practical runtime relationships: what depends on what, who calls whom, and where the data goes next.

### A. Solution & Project Roots

### `real-design-2.sln`
- Purpose: Top-level solution container for the hosted architecture.
- Inputs/Outputs: Inputs Visual Studio/MSBuild project metadata; outputs coordinated multi-project build and debugging.
- Interactions: Includes `RealDesign2.Client`, `RealDesign2.Server`, and `RealDesign2.Shared`.

### `Client/RealDesign2.Client.csproj`
- Purpose: Defines the browser application, package references, and project reference to `Shared`.
- Inputs/Outputs: Inputs NuGet package references and source files; outputs the WASM client build.
- Interactions: References `Shared/RealDesign2.Shared.csproj`.

### `Server/RealDesign2.Server.csproj`
- Purpose: Defines the hosting/API application and references `Client` and `Shared`.
- Inputs/Outputs: Inputs server source plus referenced projects; outputs the host executable.
- Interactions: Hosts the built client and compiles against shared DTOs.

### `Shared/RealDesign2.Shared.csproj`
- Purpose: Defines the common DTO/model assembly.
- Inputs/Outputs: Inputs model classes; outputs `RealDesign2.Shared.dll`.
- Interactions: Referenced by both `Client` and `Server`.

### B. Client Startup, Routing, and Layout

### `Client/Program.cs`
- Purpose: Browser bootstrap and DI composition root.
- Inputs/Outputs: Inputs app configuration from `wwwroot/appsettings.json`; outputs the running WASM host.
- Interactions:
  - Registers `ApiAuthService`, `ApiModuleService`, `CustomAuthStateProvider`, `HrMockDataService`
  - Mounts `App.razor`
  - Configures shared `HttpClient`

### `Client/App.razor`
- Purpose: Global routing, authorization gating, not-found handling, and error boundary rendering.
- Inputs/Outputs: Inputs `AuthenticationState` and route metadata; outputs the resolved page layout/page component.
- Interactions:
  - Uses `MainLayout`
  - Uses `RedirectToLogin`
  - Wraps all routed pages in `AuthorizeRouteView`
  - Depends on `CustomAuthStateProvider` via Blazor auth services

### `Client/_Imports.razor`
- Purpose: Global Razor imports and default authorization attribute.
- Inputs/Outputs: Inputs namespace references; outputs simpler Razor files with shared using scope.
- Interactions: Applies `[Authorize]` to pages by default, forcing explicit opt-out such as `Login.razor`.

### `Client/Layout/MainLayout.razor`
- Purpose: Shared authenticated application shell with app bar, responsive drawer, providers, and body host.
- Inputs/Outputs: Inputs auth state and navigation events; outputs the frame around most application pages.
- Interactions:
  - Renders `NavMenu`
  - Reads `IAuthService.CurrentUser`
  - Calls `LogoutAsync()` on `IAuthService`
  - Subscribes to `NavigationManager.LocationChanged`
  - Uses `MudBreakpointProvider` to adjust drawer behavior

### `Client/Layout/EmptyLayout.razor`
- Purpose: Minimal layout for screens that should not show the full app shell.
- Inputs/Outputs: Inputs child content; outputs a bare layout.
- Interactions: Used by `Login.razor`.

### `Client/Layout/NavMenu.razor`
- Purpose: Left navigation generation based on current authorized modules.
- Inputs/Outputs: Inputs `IAuthService.CurrentUser` and `ApiModuleService` module results; outputs `MudNavLink`/`MudNavGroup` navigation UI.
- Interactions:
  - Calls `IModuleService.GetAllowedModulesAsync()`
  - Subscribes to `IAuthService.OnAuthStateChanged`
  - Rendered inside `MainLayout.razor`

### `Client/RedirectToLogin.razor`
- Purpose: Route guard helper that redirects anonymous users to `/login` with a safe `returnUrl`.
- Inputs/Outputs: Inputs current URI; outputs a client navigation redirect.
- Interactions: Called from `App.razor` when `AuthorizeRouteView` rejects an anonymous user.

### `Client/Theme/AppTheme.cs`
- Purpose: Central MudBlazor theme definition.
- Inputs/Outputs: Inputs static color and typography choices; outputs a `MudTheme`.
- Interactions: Consumed by `MainLayout.razor` through `MudThemeProvider`.

### C. Client Auth, Session, and API Services

### `Client/Interfaces/IAuthService.cs`
- Purpose: Auth abstraction for login/logout/current-user state.
- Inputs/Outputs: Inputs credentials and cancellation tokens; outputs login result tuple and auth-state events.
- Interactions: Implemented by `ApiAuthService`; consumed by `Login.razor`, `MainLayout.razor`, `NavMenu.razor`, `CustomAuthStateProvider`.

### `Client/Interfaces/IModuleService.cs`
- Purpose: Module retrieval abstraction.
- Inputs/Outputs: Inputs cancellation tokens; outputs allowed or full module lists.
- Interactions: Implemented by `ApiModuleService`; consumed by `NavMenu.razor` and `Home.razor`.

### `Client/Services/IAuthSessionStore.cs`
- Purpose: Minimal storage contract for the active token and user.
- Inputs/Outputs: Inputs token/user assignments; outputs current in-memory session snapshot.
- Interactions: Implemented by `InMemoryAuthSessionStore`; consumed by `ApiAuthService`.

### `Client/Services/InMemoryAuthSessionStore.cs`
- Purpose: In-process browser session store for `AccessToken` and `CurrentUser`.
- Inputs/Outputs: Inputs auth values from `ApiAuthService`; outputs current auth snapshot.
- Interactions: Registered in DI in `Client/Program.cs`, consumed only by `ApiAuthService`.

### `Client/Services/ApiAuthService.cs`
- Purpose: Login/logout orchestration and bearer token header management.
- Inputs/Outputs:
  - Inputs username/password and the current `HttpClient`
  - Outputs `UserDto`, auth events, and updated authorization headers
- Interactions:
  - Calls `Server/Controllers/AuthController.cs`
  - Persists auth state to `InMemoryAuthSessionStore`
  - Raises events consumed by `CustomAuthStateProvider`, `NavMenu.razor`, `MainLayout.razor`

### `Client/Services/CustomAuthStateProvider.cs`
- Purpose: Converts `UserDto` into Blazor `AuthenticationState`.
- Inputs/Outputs:
  - Inputs `UserDto?` from `IAuthService`
  - Outputs `ClaimsPrincipal` for the Blazor authorization system
- Interactions:
  - Depends on `IAuthService`
  - Drives `[Authorize]`, `AuthorizeView`, and `AuthorizeRouteView`

### `Client/Services/ApiModuleService.cs`
- Purpose: Reads module metadata from the server API.
- Inputs/Outputs:
  - Inputs the shared `HttpClient`
  - Outputs `IReadOnlyList<ModuleDefinitionDto>`
- Interactions:
  - Calls `api/modules`
  - Calls `api/modules/all`
  - Consumed by `NavMenu.razor` and `Home.razor`

### `Client/Services/HrMockDataService.cs`
- Purpose: Supplies browser-side mock master data for the HR dynamic CRUD module.
- Inputs/Outputs: Inputs nothing; outputs lists of `Sector`, `Department`, `Job`, `Qualification`, `Section`, `AttendanceSystem`, and `Shift`.
- Interactions: Consumed by `Client/Pages/Hr.razor.cs`.

### D. Shared Client UI Primitives

### `Client/Shared/PageContainer.razor`
- Purpose: Central page wrapper for consistent padding and page titles.
- Inputs/Outputs: Inputs `Title` and `ChildContent`; outputs a normalized page shell.
- Interactions: Used by `Home.razor`, `Hr.razor`, and `ModulePlaceholder.razor`.

### `Client/Shared/FormActionToolbar.razor`
- Purpose: Reusable CRUD action bar with Add/Modify/Delete/Save/Close actions.
- Inputs/Outputs: Inputs `EventCallback`s; outputs a standardized MudBlazor toolbar.
- Interactions: Rendered by `DynamicCrudView.razor`.

### `Client/Shared/DynamicCrudView.razor`
- Purpose: Shared CRUD wrapper that standardizes table + form composition.
- Inputs/Outputs:
  - Inputs `Title`, `BadgeText`, `Items`, `HeaderContent`, `RowTemplate`, `ChildContent`, and toolbar callbacks
  - Outputs a consistent table-and-form screen
- Interactions:
  - Uses `FormActionToolbar.razor`
  - Used extensively by `Hr.razor`

### `Client/ModulePlaceholder.razor`
- Purpose: Generic module shell for unfinished business areas.
- Inputs/Outputs: Inputs `Title` and `Icon`; outputs a page shell with icon and back-to-dashboard action.
- Interactions:
  - Uses `PageContainer.razor`
  - Used by `Pos.razor`, `Inventory.razor`, `Sales.razor`, `Purchases.razor`, `Crm.razor`, and `Exports.razor`

### E. Client Pages

### `Client/Pages/Home.razor`
- Purpose: Authenticated landing dashboard showing the modules the current user may access.
- Inputs/Outputs:
  - Inputs `AuthenticationStateProvider`, `IModuleService`, and `NavigationManager`
  - Outputs the bento-style module card grid
- Interactions:
  - Calls `ApiModuleService`
  - Reads claims built by `CustomAuthStateProvider`
  - Uses `PageContainer.razor`
  - Navigates to module routes

### `Client/Pages/Login.razor`
- Purpose: Sign-in screen for JWT acquisition.
- Inputs/Outputs:
  - Inputs username/password via bound form
  - Outputs login attempts, route redirect on success, validation/error UI on failure
- Interactions:
  - Calls `IAuthService.LoginAsync()`
  - Uses `EmptyLayout.razor`
  - Redirects into authorized routes on success

### `Client/Pages/Hr.razor`
- Purpose: Dynamic HR admin surface composed from a left module menu and reusable CRUD shells.
- Inputs/Outputs:
  - Inputs current `_activeForm` state and per-form draft models
  - Outputs the rendered CRUD screen for the selected HR section
- Interactions:
  - Uses `PageContainer.razor`
  - Uses `DynamicCrudView.razor`
  - Uses shared HR DTOs from `Shared/Models/HrDtos.cs`
  - Delegates behavior/state mutations to `Hr.razor.cs`

### `Client/Pages/Hr.razor.cs`
- Purpose: Behavioral code-behind for the HR module.
- Inputs/Outputs:
  - Inputs mock data service results, user actions, and draft state
  - Outputs mutated in-memory collections, snackbars, and rendered HR state
- Interactions:
  - Depends on `HrMockDataService`
  - Depends on `ISnackbar`
  - Supplies the backing state for `Hr.razor`

### `Client/Pages/Pos.razor`
### `Client/Pages/Inventory.razor`
### `Client/Pages/Sales.razor`
### `Client/Pages/Purchases.razor`
### `Client/Pages/Crm.razor`
### `Client/Pages/Exports.razor`
- Purpose: Authorized placeholder routes for unfinished modules.
- Inputs/Outputs: Inputs only route activation; outputs `ModulePlaceholder`.
- Interactions: Each instantiates `ModulePlaceholder.razor` with a different title/icon.

### `Client/Pages/Error.razor`
- Purpose: Conventional error route target.
- Inputs/Outputs: Outputs route-based error UI.
- Interactions: Used indirectly by server error handling and routing.

### `Client/Pages/Counter.razor`
### `Client/Pages/Weather.razor`
- Purpose: Template/demo pages retained from standard Blazor scaffolding.
- Inputs/Outputs: Outputs isolated demo UIs.
- Interactions: Not part of the core ERP flow.

### F. Server API Layer

### `Server/Program.cs`
- Purpose: ASP.NET Core host bootstrap, JWT setup, controller hosting, and WASM static file hosting.
- Inputs/Outputs: Inputs `appsettings.json` and code registrations; outputs the running API/web host.
- Interactions:
  - Configures `JwtOptions`
  - Registers `MockUserStore`, `MockModuleCatalogService`, `JwtTokenService`
  - Maps `AuthController` and `ModulesController`

### `Server/Controllers/AuthController.cs`
- Purpose: Auth API surface for login, current-user resolution, and logout.
- Inputs/Outputs:
  - Inputs `LoginRequestDto` or authenticated `ClaimsPrincipal`
  - Outputs `LoginResponseDto`, `UserDto`, or `401/204`
- Interactions:
  - Calls `MockUserStore`
  - Calls `JwtTokenService`
  - Called by `ApiAuthService`

### `Server/Controllers/ModulesController.cs`
- Purpose: Module catalog API constrained by JWT claims/roles.
- Inputs/Outputs:
  - Inputs `AllowedModuleId` and role claims from the authenticated user
  - Outputs filtered or full `ModuleDefinitionDto` collections
- Interactions:
  - Calls `MockModuleCatalogService`
  - Called by `ApiModuleService`

### G. Server Services and Configuration

### `Server/Options/JwtOptions.cs`
- Purpose: Strongly typed configuration binding for JWT settings.
- Inputs/Outputs: Inputs configuration values; outputs typed properties for issuer/audience/key/expiration.
- Interactions: Bound in `Server/Program.cs`, consumed by `JwtTokenService`.

### `Server/Services/JwtTokenService.cs`
- Purpose: JWT construction and packaging of the login response.
- Inputs/Outputs:
  - Inputs `UserDto` and configured signing metadata
  - Outputs `LoginResponseDto` with signed token and expiry
- Interactions:
  - Consumed by `AuthController`
  - Depends on `JwtOptions`

### `Server/Services/MockUserStore.cs`
- Purpose: Mock identity repository and credential validator.
- Inputs/Outputs:
  - Inputs username/password or user ID
  - Outputs cloned `UserDto` instances or `null`
- Interactions:
  - Consumed by `AuthController`
  - Acts as the authoritative source of allowed modules per user

### `Server/Services/MockModuleCatalogService.cs`
- Purpose: Mock module metadata catalog and filtering service.
- Inputs/Outputs:
  - Inputs allowed module IDs
  - Outputs cloned `ModuleDefinitionDto` lists
- Interactions:
  - Consumed by `ModulesController`
  - Defines route, icon, and color metadata used by `Home.razor` and `NavMenu.razor`

### `Server/appsettings.json`
- Purpose: Runtime host configuration including JWT settings.
- Inputs/Outputs: Inputs deployment/runtime configuration; outputs bound configuration values for the server host.
- Interactions:
  - Read by `Server/Program.cs`
  - Binds the `Jwt` section to `JwtOptions`

### `Server/appsettings.Development.json`
- Purpose: Development-time overrides.
- Inputs/Outputs: Overrides environment-specific values when running in Development.
- Interactions: Merged into server configuration by ASP.NET Core.

### `Server/Properties/launchSettings.json`
- Purpose: Local development launch profiles.
- Inputs/Outputs: Inputs IDE/debug settings; outputs predictable local launch behavior.
- Interactions: Used by local `dotnet run`/Visual Studio tooling.

### H. Shared Contract Layer

### `Shared/Models/UserDto.cs`
- Purpose: Shared user identity and authorization payload.
- Inputs/Outputs:
  - Inputs mock user construction or API serialization
  - Outputs user information and allowed module IDs
- Interactions:
  - Produced by `MockUserStore`
  - Returned by `AuthController`
  - Consumed by `ApiAuthService` and `CustomAuthStateProvider`

### `Shared/Models/ModuleDefinitionDto.cs`
- Purpose: Shared module metadata contract.
- Inputs/Outputs: Inputs mock catalog definitions; outputs module render metadata for the client.
- Interactions:
  - Produced by `MockModuleCatalogService`
  - Consumed by `NavMenu.razor` and `Home.razor`

### `Shared/Models/LoginRequestDto.cs`
- Purpose: Login API request contract.
- Inputs/Outputs: Inputs username/password with validation annotations; outputs serialized JSON payload for `/api/auth/login`.
- Interactions: Sent by `ApiAuthService`, received by `AuthController`.

### `Shared/Models/LoginResponseDto.cs`
- Purpose: Login API response contract.
- Inputs/Outputs: Inputs token/expiry/user server-side; outputs strongly typed auth payload client-side.
- Interactions: Created by `JwtTokenService`, consumed by `ApiAuthService`.

### `Shared/Models/HrDtos.cs`
- Purpose: Shared HR CRUD models and base DTO hierarchy.
- Inputs/Outputs: Inputs mock service and page state; outputs strongly typed HR records for the dynamic CRUD UI.
- Interactions:
  - Used by `HrMockDataService`
  - Bound in `Hr.razor` and `Hr.razor.cs`

## Phase 4: Centralization & Global Theming Guide

This is the practical “change it in one place” guide for future developers.

### 1. Global Page Padding and Headers: `Client/Shared/PageContainer.razor`

#### What it centralizes
- page-level title rendering
- the outer content padding for major pages
- consistent page vertical rhythm

#### How to change the entire app’s page spacing
Edit:

- `Client/Shared/PageContainer.razor`
- `Client/wwwroot/app.css`

Key hooks:

- markup wrapper: `.page-container`
- content wrapper: `.page-container__content`
- title text: `.page-container__title`

Practical examples:

- To make all pages tighter, reduce `.page-container__content` padding in `app.css`.
- To make every page title larger or bolder, change `.page-container__title`.
- To add a subtitle pattern globally, extend `PageContainer.razor` once and reuse it everywhere.

### 2. Global CRUD Buttons: `Client/Shared/FormActionToolbar.razor`

#### What it centralizes
- button order
- icons
- colors
- spacing/wrapping
- the presence of the close affordance

#### How to restyle all CRUD screens at once
Edit:

- `Client/Shared/FormActionToolbar.razor`
- `Client/wwwroot/app.css`

High-impact properties:

- `Variant`
- `Color`
- `StartIcon`
- button text labels
- the `MudStack` wrapping and justification

High-impact CSS classes:

- `.form-action-toolbar`
- `.form-action-toolbar__button`
- `.form-action-toolbar__button--primary`
- `.form-action-toolbar__close`

Practical examples:

- To make Save use a gradient or stronger shadow, update the primary button class.
- To shrink all CRUD controls for denser forms, adjust `.form-action-toolbar__button`.
- To add a new standard action such as Export, add it here once and every `DynamicCrudView` gains it after wiring.

### 3. Global Data Grids and Form Shells: `Client/Shared/DynamicCrudView.razor`

#### What it centralizes
- table shell structure
- table border/elevation policy
- title/badge header pattern
- form container structure
- shared toolbar placement
- horizontal scrolling behavior for dense tables

#### How to change all CRUD screens at once
Edit:

- `Client/Shared/DynamicCrudView.razor`
- `Client/wwwroot/app.css`

Important markup parameters:

- `Title`
- `BadgeText`
- `Items`
- `HeaderContent`
- `RowTemplate`
- `ChildContent`

Important `MudTable` behaviors:

- `Hover="true"`
- `Dense="true"`
- `HorizontalScrollbar="true"`
- `Breakpoint="Breakpoint.None"`

Important CSS classes:

- `.dynamic-crud-view__table-shell`
- `.dynamic-crud-view__table`
- `.dynamic-crud-view__chip`
- `.dynamic-crud-view__form-shell`
- `.dynamic-crud-view__form-body`

Practical examples:

- To make all grids more spacious, disable `Dense` or increase table cell padding in CSS.
- To change the header band appearance for every CRUD table, edit `.dynamic-crud-view__table .mud-table-head .mud-table-cell`.
- To change the default layout relationship between grid and form, modify `DynamicCrudView.razor` once instead of updating each HR section.

### 4. Global Visual Language: `Client/wwwroot/app.css` and `Client/Theme/AppTheme.cs`

#### `app.css` is the global CSS override layer
Use this file for:

- shared utility classes
- custom shadows, borders, rounded corners
- app shell styling
- PageContainer, CRUD view, and toolbar styling
- responsive tweaks not easily expressed through MudBlazor parameters

This is the best place to change:

- glassmorphism shell appearance
- card hover behavior
- page spacing
- shared component class styling

#### `AppTheme.cs` is the MudBlazor design-token layer
Use this file for:

- palette colors
- app bar base palette color
- typography defaults
- MudBlazor-native theming behavior

This is the best place to change:

- brand primary color
- background/surface defaults
- typography stack
- future global palette semantics

### Recommended Change Strategy

When a developer wants to change styling globally, use this order:

1. `AppTheme.cs` for palette/typography-level changes.
2. `PageContainer.razor` for global page framing and headers.
3. `DynamicCrudView.razor` for all CRUD screen structure.
4. `FormActionToolbar.razor` for all CRUD actions.
5. `app.css` for class-level styling, polish, responsiveness, and special effects.

This keeps the system centralized instead of drifting into per-page styling patches.
