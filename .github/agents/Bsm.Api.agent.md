---
name: Bsm.Api
description: Build and maintain the Bsm.Api ASP.NET Core Web API using controller-based endpoints and clean, focused changes.
---

# Bsm.Api Agent

Use this agent for work in the `Bsm.Api` project.

## Project Guidelines

- Target `net10.0` and preserve nullable reference types.
- Use Vertical Slice Architecture: organize code by business feature, not by technical layer.
- Keep each feature's endpoint, request/response models, handler, validation, and mapping together.
- Keep feature slices under `Bsm.Api/Features/<FeatureName>`.
- Use thin controllers only when controller-based routing is required; inherit them from `ApiControllerBase`.
- Use attribute routing with the `api/[controller]` route convention for controllers.
- Use asynchronous endpoint methods when calling I/O-bound services.
- Keep business logic inside the relevant feature slice; avoid a shared service layer unless behavior is genuinely cross-cutting.
- Return appropriate HTTP status codes and use strongly typed response models.
- Use ASP.NET Core Identity as the default authentication and user-management system.
- Use `ApplicationUser` as the custom `IdentityUser` type, together with the built-in `IdentityRole`, claims, policies, and framework authentication handlers.
- Keep Identity configuration and persistence wiring centralized, while keeping authorization rules close to the feature that uses them.
- Avoid adding template sample endpoints or unrelated dependencies.

## Validation

After code changes, run:

```powershell
dotnet restore .\Bsm.Api\Bsm.Api.csproj
dotnet build .\Bsm.Api\Bsm.Api.csproj --no-restore
```

Run focused tests when tests are added to the solution.