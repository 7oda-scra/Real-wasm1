# Lead Developer Presentation: Modern Blazor ERP UI

## 1. The Goal: Modernizing Legacy to Web
The objective was to architect a high-end, responsive replacement for legacy ERP systems. We prioritized a "Web-First" experience that captures the power of desktop accounting software while leveraging the accessibility and aesthetic of modern SaaS platforms (Stripe/Apple).

## 2. Core Architecture: Blazor WASW & MudBlazor
- **Platform**: **.NET 9 Blazor WebAssembly** (currently running in Interactive Server mode for rapid prototyping).
- **UI Framework**: **MudBlazor**. We leverage its robust component library but heavily customize the theme (Primary: `#44abdf`, Background: `#F9FAFB`) to create a bespoke brand feel.
- **DTO-Driven UI**: The entire navigation and dashboard are dynamic. The frontend consumes `ModuleDefinitionDto` lists, ensuring the UI automatically adapts to user permissions without hardcoded menu items.

## 3. Security Infrastructure
- **CustomAuthStateProvider**: A centralized state manager that bridges our `MockAuthService` and Blazor's native security pipeline. It maps custom DTO claims (like `AllowedModuleId`) into a standard `ClaimsPrincipal`.
- **Global Authorization**: Implemented a "Secure by Default" posture via `_Imports.razor` using `@attribute [Authorize]`. This forces a global opt-out strategy, ensuring all new pages are protected automatically.
- **Router Interception**: The `Routes.razor` file is wrapped in an `AuthorizeRouteView`, elegantly handling redirection to `/login` for unauthenticated sessions.

## 4. The UX Upgrade: Bento & Master-Detail
- **Bento Box Dashboard**: A modern, high-density landing page using responsive grid layouts. Each module is represented by an oversized, statically-branded icon badge with dynamic hover-lift effects.
- **Master-Detail HR Layout**: Solved the "Complex Form" problem by implementing a vertical navigation master menu on the left and a dynamic staging area on the right. 
- **Responsive Polish**: The HR layout is viewport-aware. On mobile, the navigation collapses into a "Back to Menu" flow to maximize data entry space for the user.

## 5. Deployment Preparedness
- **Netlify Ready**: Includes SPA routing (`_redirects`) and a optimized publisher pipeline.
- **Standardized Styling**: All custom animations and bento card aesthetics are centralized in `app.css` to bypass Blazor's CSS isolation attribute overhead in global interactive trees.
