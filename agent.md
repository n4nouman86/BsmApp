# BsmJoinApp Project Guide

## Product direction

- BsmJoin is the login, identity, and user-management application maintained in this repository.

## Current repository

- `Bsm.Api/` is an ASP.NET Core .NET 10 Web API using controllers, ASP.NET Core Identity, EF Core with SQL Server, and JWT bearer authentication.
- `Bsm.WebUI/` is a React + TypeScript application built with Vite.
- The Web UI calls the API at `http://localhost:5049/api` by default. Configure `VITE_API_URL` when the API has a different URL.
- Admin user-management endpoints are protected by the `Admin` role.
- Users directory search sends `GET /api/users?search=...` only on Enter, matching full name or email. Clearing the input and pressing Enter restores all users; typing alone does not filter or send requests.
- Password creation/reset requires at least 6 characters only, with no case, digit, symbol, uniqueness or explicit maximum-length rules.
- Admins can block/unblock users at `PUT /api/users/{id}/blocked`. Blocked users cannot log in; every protected API request checks `IsBlocked` and JWT `token_version` against the database. Blocking/unblocking increments `TokenVersion`, so old sessions remain revoked after unblock. Self-block and blocking the last unblocked Admin are rejected. Already-running requests are not cancelled.
- Development startup ensures the `Admin` and `Verifier` Identity roles exist. User create/edit forms select one role via inline radios, Verifier first and default on create. API rejects simultaneous Admin/Verifier assignments; omitted role flags preserve existing assignments on update. `Verifier` has no additional permissions.
- Leads map the existing `dbo.Leads` table. Signed-in users manage their own leads; Admin can manage all leads. New leads belong to their creator, and deletion is permanent after confirmation. Every lead has a required `StatusId` FK to `dbo.LeadStatuses`.
- Lead statuses map `dbo.LeadStatuses` (`/api/leadstatuses`): everyone can read; only Admin can create, edit or delete. Deleting a status that leads still use returns 409.

## Feature structure

- Organize both `Bsm.Api/` and `Bsm.WebUI/` directly under their `Features/` directories.
- Keep each API feature's endpoints, contracts, and data access under `Bsm.Api/Features/<Feature>/`, and its related pages and UI logic under `Bsm.WebUI/src/Features/<Feature>/`.
- Keep a feature's controllers/endpoints, contracts, services, data access, registration, and request examples together in its API slice. Keep its route/page and app-specific UI logic together in its WebUI slice.
- Put only genuinely reusable app-shell and presentation components in `Bsm.WebUI/src/Shared/`. Keep cross-app infrastructure outside feature folders only when it is genuinely shared.
- When adding a new use case, add its files directly to a feature folder under each project's `Features/` directory where applicable; avoid app-name folders, root-level app-specific files, or placeholder features without an implemented requirement.

## Development

- Run the API from the repository root: `dotnet run --project Bsm.Api/Bsm.Api.csproj --launch-profile http`.
- Before deploying user blocking, run `Bsm.Api/Identity/AddUserBlocking.sql` against the Identity database. The idempotent script adds `IsBlocked` and `TokenVersion` with safe defaults for existing users. Tokens issued before this change lack a version and require fresh login.
- User first/last names are limited to 50 characters in SQL, EF, API validation and forms. Apply `Bsm.Api/Identity/LimitUserNames.sql` to existing databases; it rejects overlength data rather than truncating it.
- User `CreatedOn` is stored as UTC `datetime2(7)` with an EF converter preserving `DateTimeOffset` API responses. Apply `Bsm.Api/Identity/ConvertUserCreatedOn.sql` to existing databases; it normalizes offsets to UTC before conversion. Identity `LockoutEnd` remains `datetimeoffset`.
- Run the Web UI from `Bsm.WebUI/`: `npm run dev`.
- Build and lint the Web UI from `Bsm.WebUI/`: `npm run build` and `npm run lint`.
- Build the API from the repository root: `dotnet build Bsm.Api/Bsm.Api.csproj`.
- Local development may use SQL Server configured in `Bsm.Api/appsettings.json` and development credentials in `Bsm.Api/appsettings.Development.json`. Never commit real credentials or production secrets.

## Implementation conventions

- Follow existing patterns: controller-based API endpoints, Identity for user/password operations, and typed React components.
- Keep user-facing data API-backed; do not add fabricated dashboard, activity, billing, or directory records.
- Preserve the shared slate-gray styling with a charcoal sidebar, Apple system font stack, consistent `MMM d, yyyy` date format, sticky navigation, and responsive behavior.
- Reuse `Bsm.WebUI/src/session.ts` for shared API URL, access token, current-user, and date-format helpers.
- Keep browser titles descriptive and branded as `... | bsm`; use `bsm` for visible UI branding.
- Validate relevant changes with the smallest applicable build/lint checks.
