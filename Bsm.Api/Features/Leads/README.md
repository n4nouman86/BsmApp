# Leads

The API maps the existing `BsmApp.dbo.Leads` table; it does not create, migrate,
or seed lead records. SQL column spelling (`AnyLaspse`, `Tikets`, `HopeType`) is
preserved. All varchar limits and nullable columns match the supplied schema.
Birth dates use date-only ISO values (`yyyy-MM-dd`), not timestamps. New
`CreatedOn` values are UTC and responses include the UTC offset.

All endpoints require authentication. Users can list, read, update, and
permanently delete only their own leads. Administrators can manage all leads,
including records without an owner. Inaccessible IDs return 404. Creating a lead
always assigns the authenticated user's ID; neither owner nor creation timestamp
can be supplied or changed by the client.

Every lead requires a `statusId` that references `dbo.LeadStatuses` (foreign key,
not null). Unknown status IDs return 400, responses include `statusId` and
`statusName`, and deleting a status that leads still use returns 409.

The Web UI at `/leads` includes searchable listings, full details, create/edit
forms, and permanent-delete confirmation. Non-admin users land on Leads rather
than the administrator-only user overview.

Use [Leads.http](./Leads.http) for request examples. POST returns 201 and a
Location header, PUT returns 200, and DELETE returns 204. Invalid input returns
400 with validation details. Authentication failures return 401.

With the local API running and `DevelopmentAdmin` configured, run
`node --test tests\leads.integration.mjs` from the repository root. This checks
CRUD, field limits, nullable fields, server-assigned ownership/timestamps, and
cross-user isolation against the real table. It creates two temporary users, two
temporary statuses and synthetic leads, then deletes only those test records in a cleanup block.
