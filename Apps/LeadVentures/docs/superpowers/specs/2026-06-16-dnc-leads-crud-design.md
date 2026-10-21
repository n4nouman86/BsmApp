# DncLeads CRUD — Design Spec
Date: 2026-06-16

## Overview
Add CRUD operations for the existing `DncLeads` SQL table. Follows the exact Invoice pattern: code-behind split, `OpenSideAsync` for Create/Edit, `dialogService.Confirm()` for Delete, in-memory list update with new list reference to trigger grid re-render.

## Requirements
- Role: Super Admin only
- Navigation: Under existing "Leads" menu, item named "DNC", icon "block"
- Route: `/DncLeadIndex`
- Table already exists in SQL Server — no migration needed (empty migration if EF requires one)

## Entity
File: `Entities/DncLead.cs`

```
int Id          (IDENTITY, PK)
string Phone    (varchar 50, required)
string? FirstName
string? LastName
string? Address1
string? City
string? State
string? PostalCode
string? Email
```

Attributes: `#nullable disable`, `[Key]`, `[Required]`, `[StringLength]`, `[Unicode(false)]` — matches existing entity conventions.

## Files

| File | Notes |
|------|-------|
| `Entities/DncLead.cs` | Entity class |
| `Components/Pages/DncLeadPages/IndexPage.razor` | List page, search by Phone/Name |
| `Components/Pages/DncLeadPages/IndexPage.razor.cs` | OpenCreate, OpenEdit, Delete, list state |
| `Components/Pages/DncLeadPages/CreatePage.razor` | Side panel form, no `@page` |
| `Components/Pages/DncLeadPages/CreatePage.razor.cs` | Save → CloseSide(Input.Id) |
| `Components/Pages/DncLeadPages/EditPage.razor` | Side panel form, `[Parameter] int DncLeadId` |
| `Components/Pages/DncLeadPages/EditPage.razor.cs` | Load, Save → CloseSide(Input.Id) |
| `Data/ApplicationDbContext.cs` | Add DbSet<DncLead> |
| `Components/Layout/NavMenu.razor` | Add DNC item under Leads |

## Patterns (from Invoice pages)
- `dialogService.OpenSideAsync<CreatePage>("Add DNC Lead", options: new SideDialogOptions { Position = DialogPosition.Right, ShowMask = true, MinWidth = 450, Width = "600px" })`
- `dialogService.OpenSideAsync<EditPage>("Edit DNC Lead", new Dictionary<string, object?> { { "DncLeadId", id } }, options)`
- After create: `DncLeads?.Insert(0, newRecord); DncLeads = new List<DncLead>(DncLeads!);`
- After edit: find by index, replace, `DncLeads = new List<DncLead>(DncLeads);`
- Delete: `dialogService.Confirm()` → `context.DncLeads.Where(x => x.Id == id).ExecuteDeleteAsync()` → remove from list
- `DialogService.CloseSide(Input.Id)` on save, `DialogService.CloseSide(false)` on cancel

## Search
Index page will have a simple search fieldset: Phone (partial match) and Name (FirstName/LastName contains).
