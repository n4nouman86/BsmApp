# React + TypeScript + Vite

## Companion admin dashboards

API code is organized by vertical slice under `BsmJoin.Api/Features/`:
`BsmJoin/Auth/`, `BsmJoin/Users/`, `LeadLogix/Dashboard/`,
`LeadLogix/User/`, `LeadVentures/Dashboard/`, and `LeadVentures/User/`.
Each dashboard slice keeps its controller, contracts, report service, and
database context together. WebUI features use the same layout under
`src/Features/`, with reusable dashboard presentation and application shell
components in `src/Shared/`.

The `/leadlogix` and `/leadventures` admin dashboards load authenticated,
read-only reports through `BsmJoin.Api` at `/api/{app}/dashboard`. An `Admin`
login is required. Each app uses its own existing `vwcTotalDayTransfers` view,
`spTransferDailyReport`, and `spGetTransferWeeklyReport` procedures; neither
creates or seeds app data, alters its schema, or uses the BsmJoin identity
database for reports.

In API `Development`, the connection is read from the existing local
`Apps/LeadLogix/BlazorPortal/appsettings.json` and
`appsettings.Development.json` for LeadLogix, and
`Apps/LeadVentures/BlazorPortal/appsettings.json` and
`appsettings.Development.json` for LeadVentures (development settings take
precedence). To override a connection, or when deploying, configure
`ConnectionStrings__LeadLogixConnection` and
`ConnectionStrings__LeadVenturesConnection` on the API server. Never put
connection strings in Vite variables or commit credentials. Deployments must
provide their own connection strings; imported settings are only read in development.
Report model files are linked from LeadLogix during the API build because both
apps use the same report result definitions, without referencing either app's
executable.

The chart displays the latest seven **recorded** transfer dates, not seven
fabricated calendar days. The day report defaults to the latest recorded date,
and the weekly report defaults to that date's Monday-through-Sunday week. Pick
dates present in your database to see historical reports. Empty dates show an
empty state, and connection failures show an error with Retry. Weekly date
ranges are inclusive. The UI formats schedule times in the browser's local timezone.
API request examples, including validation and authorization checks, are in
`BsmJoin.Api/Features/LeadLogix/Dashboard/LeadLogix.http` and
`BsmJoin.Api/Features/LeadVentures/Dashboard/LeadVentures.http`.

## Local demo login

Run the API in `Development` and start this UI with `npm run dev`.
The API seeds a local demo administrator with email `demo@bsmjoin.test`
and initial password `Demo@12345`. The development login page shows these
credentials. Sign in using the email address.

Normal user password validation is unchanged. The API rejects this account
outside `Development`, and production UI builds do not show the demo hint.
These are public test credentials; never use this account for real data.

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the ESLint configuration

If you are developing a production application, we recommend updating the configuration to enable type-aware lint rules:

```js
export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...

      // Remove tseslint.configs.recommended and replace with this
      tseslint.configs.recommendedTypeChecked,
      // Alternatively, use this for stricter rules
      tseslint.configs.strictTypeChecked,
      // Optionally, add this for stylistic rules
      tseslint.configs.stylisticTypeChecked,

      // Other configs...
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])

```

You can also install [eslint-plugin-react-x](https://npmx.dev/package/eslint-plugin-react-x) and [eslint-plugin-react-dom](https://npmx.dev/package/eslint-plugin-react-dom) for React-specific lint rules:

```js
// eslint.config.js
import reactX from 'eslint-plugin-react-x'
import reactDom from 'eslint-plugin-react-dom'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...
      // Enable lint rules for React
      reactX.configs['recommended-typescript'],
      // Enable lint rules for React DOM
      reactDom.configs.recommended,
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])

```
