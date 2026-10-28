# BsmJoinApp Project Guide

## Product direction

- BsmJoin is the login, identity, and user-management application maintained in this repository.

## Current repository

- `Bsm.Api/` is an ASP.NET Core .NET 10 Web API using controllers, ASP.NET Core Identity, EF Core with SQL Server, and JWT bearer authentication.
- `Bsm.WebUI/` is a React + TypeScript application built with Vite.
- The Web UI calls the API at `http://localhost:5049/api` by default. Configure `VITE_API_URL` when the API has a different URL.
- Admin user-management endpoints are protected by the `Admin` role.

## Feature structure

- Organize both `Bsm.Api/` and `Bsm.WebUI/` directly under their `Features/` directories.
- Keep each API feature's endpoints, contracts, and data access under `Bsm.Api/Features/<Feature>/`, and its related pages and UI logic under `Bsm.WebUI/src/Features/<Feature>/`.
- Keep a feature's controllers/endpoints, contracts, services, data access, registration, and request examples together in its API slice. Keep its route/page and app-specific UI logic together in its WebUI slice.
- Put only genuinely reusable app-shell and presentation components in `Bsm.WebUI/src/Shared/`. Keep cross-app infrastructure outside feature folders only when it is genuinely shared.
- When adding a new use case, add its files directly to a feature folder under each project's `Features/` directory where applicable; avoid app-name folders, root-level app-specific files, or placeholder features without an implemented requirement.

## Development

- Run the API from the repository root: `dotnet run --project Bsm.Api/Bsm.Api.csproj --launch-profile http`.
- Run the Web UI from `Bsm.WebUI/`: `npm run dev`.
- Build and lint the Web UI from `Bsm.WebUI/`: `npm run build` and `npm run lint`.
- Build the API from the repository root: `dotnet build Bsm.Api/Bsm.Api.csproj`.
- Local development may use SQL Server configured in `Bsm.Api/appsettings.json` and development credentials in `Bsm.Api/appsettings.Development.json`. Never commit real credentials or production secrets.

## Implementation conventions

- Follow existing patterns: controller-based API endpoints, Identity for user/password operations, and typed React components.
- Keep user-facing data API-backed; do not add fabricated dashboard, activity, billing, or directory records.
- Preserve the shared teal styling, Apple system font stack, consistent `MMM d, yyyy` date format, sticky navigation, and responsive behavior.
- Reuse `Bsm.WebUI/src/session.ts` for shared API URL, access token, current-user, and date-format helpers.
- Keep browser titles descriptive and branded as `... | BsmJoin`.
- Validate relevant changes with the smallest applicable build/lint checks.
