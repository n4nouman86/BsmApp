# BsmJoinApp Project Guide

## Product direction

- BsmJoin is the central login and identity application for the product suite.
- LeadLogix and LeadVentures are companion applications maintained in this monorepo.
- The intended experience is one sign-in followed by an app switcher for BsmJoin, LeadLogix, and LeadVentures.
- Keep each application clearly separated in the repository and UI. Do not merge their domain features into BsmJoin.
- LeadLogix and LeadVentures retain their own domain code, identity setup, and deployment configuration under `Apps/`.
- Both companion apps also read data from the other app's database. Preserve these cross-database connections when changing either app.
- Their current ASP.NET Identity cookie authentication is not yet integrated with BsmJoin. Review authentication and deployment before choosing or implementing SSO; do not assume the current BsmJoin JWT alone provides cross-application SSO.

## Current repository

- `BsmJoin.Api/` is an ASP.NET Core .NET 10 Web API using controllers, ASP.NET Core Identity, EF Core with SQL Server, and JWT bearer authentication.
- `BsmJoin.WebUI/` is a React + TypeScript application built with Vite.
- `Apps/LeadLogix/` contains the LeadLogix ASP.NET Core Blazor application and solution.
- `Apps/LeadVentures/` contains the LeadVentures ASP.NET Core Blazor application and solution.
- Companion-app `appsettings*.json` files are local-only and gitignored because they may contain credentials. Configure them locally or use an appropriate secret store.
- The Web UI calls the API at `http://localhost:5049/api` by default. Configure `VITE_API_URL` when the API has a different URL.
- Admin user-management endpoints are protected by the `Admin` role.

## Feature structure

- Organize both `BsmJoin.Api/` and `BsmJoin.WebUI/` by vertical feature slices under `Features/{App}/{Feature}/`. Keep each app's API and UI code with that app; do not mix LeadLogix or LeadVentures features into BsmJoin.
- Use these feature paths:
  - API: `BsmJoin.Api/Features/BsmJoin/Auth/`, `BsmJoin.Api/Features/BsmJoin/Users/`, `BsmJoin.Api/Features/LeadLogix/Dashboard/`, `BsmJoin.Api/Features/LeadLogix/User/`, `BsmJoin.Api/Features/LeadVentures/Dashboard/`, and `BsmJoin.Api/Features/LeadVentures/User/`.
  - WebUI: `BsmJoin.WebUI/src/Features/BsmJoin/Auth/`, `BsmJoin.WebUI/src/Features/BsmJoin/Users/`, `BsmJoin.WebUI/src/Features/LeadLogix/Dashboard/`, `BsmJoin.WebUI/src/Features/LeadLogix/User/`, `BsmJoin.WebUI/src/Features/LeadVentures/Dashboard/`, and `BsmJoin.WebUI/src/Features/LeadVentures/User/`.
- Keep a feature's controllers/endpoints, contracts, services, data access, registration, and request examples together in its API slice. Keep its route/page and app-specific UI logic together in its WebUI slice.
- Put only genuinely reusable app-shell and presentation components in `BsmJoin.WebUI/src/Shared/`. Keep cross-app infrastructure outside feature folders only when it is genuinely shared.
- When adding a new use case, add its files to the matching slice in both projects where applicable; avoid root-level app-specific files or placeholder features without an implemented requirement.

## Development

- Run the API from the repository root: `dotnet run --project BsmJoin.Api/BsmJoin.Api.csproj --launch-profile http`.
- Run the Web UI from `BsmJoin.WebUI/`: `npm run dev`.
- Build and lint the Web UI from `BsmJoin.WebUI/`: `npm run build` and `npm run lint`.
- Build the API from the repository root: `dotnet build BsmJoin.Api/BsmJoin.Api.csproj`.
- Build LeadLogix from the repository root: `dotnet build Apps/LeadLogix/LeadLogixApp.sln`.
- Build LeadVentures from the repository root: `dotnet build Apps/LeadVentures/LeadVenturesApp.sln`.
- Local development may use SQL Server configured in `BsmJoin.Api/appsettings.json` and development credentials in `BsmJoin.Api/appsettings.Development.json`. Never commit real credentials or production secrets.

## Implementation conventions

- Follow existing patterns: controller-based API endpoints, Identity for user/password operations, and typed React components.
- Keep user-facing data API-backed; do not add fabricated dashboard, activity, billing, or directory records.
- Preserve the shared teal styling, Apple system font stack, consistent `MMM d, yyyy` date format, sticky navigation, and responsive behavior.
- Reuse `BsmJoin.WebUI/src/session.ts` for shared API URL, access token, current-user, and date-format helpers.
- Keep browser titles descriptive and branded as `... | BsmJoin`.
- Validate relevant changes with the smallest applicable build/lint checks.
