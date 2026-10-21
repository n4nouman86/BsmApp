# DncLeads CRUD Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add full CRUD (Create, Read, Update, Delete) for the existing `DncLeads` SQL table as a Blazor page under the Super Admin "Leads" menu.

**Architecture:** Single Index page at `/DncLeadIndex` with search; Create/Edit open in Radzen right-side panels via `dialogService.OpenSideAsync`; Delete uses `dialogService.Confirm`. Follows the Invoice pages pattern exactly (code-behind split, `CloseSide` result, in-memory list update with new list reference).

**Tech Stack:** ASP.NET Core Blazor Server, Radzen.Blazor, Entity Framework Core (SQL Server), `IDbContextFactory<ApplicationDbContext>`

---

## File Map

| Action | Path |
|--------|------|
| Create | `BlazorPortal/Entities/DncLead.cs` |
| Modify | `BlazorPortal/Data/ApplicationDbContext.cs` |
| Create | `BlazorPortal/Components/Pages/DncLeadPages/IndexPage.razor` |
| Create | `BlazorPortal/Components/Pages/DncLeadPages/IndexPage.razor.cs` |
| Create | `BlazorPortal/Components/Pages/DncLeadPages/CreatePage.razor` |
| Create | `BlazorPortal/Components/Pages/DncLeadPages/CreatePage.razor.cs` |
| Create | `BlazorPortal/Components/Pages/DncLeadPages/EditPage.razor` |
| Create | `BlazorPortal/Components/Pages/DncLeadPages/EditPage.razor.cs` |
| Modify | `BlazorPortal/Components/Layout/NavMenu.razor` |

---

### Task 1: Create DncLead Entity

**Files:**
- Create: `BlazorPortal/Entities/DncLead.cs`

- [ ] **Step 1: Create the entity file**

```csharp
#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Table("DncLeads")]
public partial class DncLead
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string Phone { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string FirstName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string LastName { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Address1 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string City { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string State { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string PostalCode { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Email { get; set; }
}
```

- [ ] **Step 2: Verify build**

Run in `BlazorPortal/` directory:
```
dotnet build
```
Expected: `Build succeeded` with 0 errors.

---

### Task 2: Register DbSet in ApplicationDbContext

**Files:**
- Modify: `BlazorPortal/Data/ApplicationDbContext.cs`

- [ ] **Step 1: Add DbSet after the `Lead` DbSet (around line 40)**

Find this line:
```csharp
    public  DbSet<Lead> Leads { get; set; }
```

Add immediately after:
```csharp
    public  DbSet<DncLead> DncLeads { get; set; }
```

- [ ] **Step 2: Verify build**

```
dotnet build
```
Expected: `Build succeeded` with 0 errors.

- [ ] **Step 3: Commit**

```bash
git add BlazorPortal/Entities/DncLead.cs BlazorPortal/Data/ApplicationDbContext.cs
git commit -m "feat: add DncLead entity and DbSet"
```

---

### Task 3: Create Index Page (razor)

**Files:**
- Create: `BlazorPortal/Components/Pages/DncLeadPages/IndexPage.razor`

- [ ] **Step 1: Create the folder and razor file**

```razor
@page "/DncLeadIndex"
@attribute [Authorize(Roles = "Super Admin")]
<PageTitle>DNC Leads</PageTitle>

@inject IDbContextFactory<ApplicationDbContext> contextFactory
@inject DialogService dialogService

<PageHeading HeadingText="DNC Leads" />

<RadzenStack AlignItems="AlignItems.Start">
    <RadzenButton Icon="add" Text="Add New DNC Lead" ButtonStyle="ButtonStyle.Primary" Click="OpenCreate" />
</RadzenStack>

<br />

<RadzenCard>
    <RadzenFieldset Text="Search">
        <EditForm Model="Input" OnValidSubmit="Search">
            <RadzenStack Orientation="Orientation.Horizontal" AlignItems="AlignItems.End">
                <RadzenStack Gap="0">
                    <RadzenLabel>Phone</RadzenLabel>
                    <RadzenTextBox @bind-Value="Input.Phone" class="w-100" />
                </RadzenStack>
                <RadzenStack Gap="0">
                    <RadzenLabel>Name</RadzenLabel>
                    <RadzenTextBox @bind-Value="Input.Name" class="w-100" />
                </RadzenStack>
                <RadzenStack>
                    <RadzenButton ButtonType="ButtonType.Submit" Text="Search" />
                </RadzenStack>
            </RadzenStack>
        </EditForm>
    </RadzenFieldset>
</RadzenCard>

<br />

<RadzenCard>
    @if (DncLeads is null)
    {
        <em>Loading...</em>
    }
    else
    {
        <RadzenDataGrid Data="DncLeads" Responsive AllowColumnReorder AllowColumnResize EmptyText="No DNC leads found">
            <Columns>
                <RadzenDataGridColumn Property="Phone" Title="Phone" Width="140px" />
                <RadzenDataGridColumn Property="FirstName" Title="First Name" />
                <RadzenDataGridColumn Property="LastName" Title="Last Name" />
                <RadzenDataGridColumn Property="Email" Title="Email" />
                <RadzenDataGridColumn Property="City" Title="City" />
                <RadzenDataGridColumn Property="State" Title="State" Width="80px" />
                <RadzenDataGridColumn Property="PostalCode" Title="Postal Code" Width="110px" />
                <RadzenDataGridColumn Title="Action" TextAlign="TextAlign.Center" Width="120px">
                    <Template Context="lead">
                        <RadzenButton Icon="edit" Size="ButtonSize.Small" ButtonStyle="ButtonStyle.Primary"
                            Click="@(() => OpenEdit(lead.Id))" title="Edit" />
                        <RadzenButton Icon="delete" Size="ButtonSize.Small" ButtonStyle="ButtonStyle.Danger"
                            Click="@(() => Delete(lead.Id))" title="Delete" />
                    </Template>
                </RadzenDataGridColumn>
            </Columns>
        </RadzenDataGrid>
    }
</RadzenCard>
```

---

### Task 4: Create Index Page (code-behind)

**Files:**
- Create: `BlazorPortal/Components/Pages/DncLeadPages/IndexPage.razor.cs`

- [ ] **Step 1: Create the code-behind file**

```csharp
using BlazorPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.DncLeadPages;

public partial class IndexPage
{
    private List<DncLead>? DncLeads { get; set; }

    protected override async Task OnInitializedAsync()
    {
        using var context = contextFactory.CreateDbContext();
        DncLeads = await context.DncLeads.OrderByDescending(x => x.Id).Take(50).ToListAsync();
    }

    private InputModel Input { get; set; } = new();

    private async Task Search()
    {
        using var context = contextFactory.CreateDbContext();
        var query = context.DncLeads.AsQueryable();

        if (!string.IsNullOrWhiteSpace(Input.Phone))
            query = query.Where(x => x.Phone.Contains(Input.Phone));

        if (!string.IsNullOrWhiteSpace(Input.Name))
            query = query.Where(x =>
                (x.FirstName != null && x.FirstName.Contains(Input.Name)) ||
                (x.LastName != null && x.LastName.Contains(Input.Name)));

        DncLeads = await query.OrderByDescending(x => x.Id).ToListAsync();
    }

    private async Task OpenCreate()
    {
        var result = await dialogService.OpenSideAsync<CreatePage>(
            "Add DNC Lead",
            options: new SideDialogOptions { Position = DialogPosition.Right, ShowMask = true, MinWidth = 450, Width = "600px" });

        if (result is int createdId)
        {
            using var context = contextFactory.CreateDbContext();
            var record = await context.DncLeads.FindAsync(createdId);
            if (record is not null)
            {
                DncLeads?.Insert(0, record);
                DncLeads = new List<DncLead>(DncLeads!);
            }
        }
    }

    private async Task OpenEdit(int id)
    {
        var result = await dialogService.OpenSideAsync<EditPage>(
            "Edit DNC Lead",
            new Dictionary<string, object?> { { "DncLeadId", id } },
            new SideDialogOptions { Position = DialogPosition.Right, ShowMask = true, MinWidth = 450, Width = "600px" });

        if (result is int updatedId)
        {
            using var context = contextFactory.CreateDbContext();
            var record = await context.DncLeads.FindAsync(updatedId);
            if (record is not null)
            {
                var index = DncLeads?.FindIndex(x => x.Id == updatedId) ?? -1;
                if (index >= 0)
                {
                    DncLeads![index] = record;
                    DncLeads = new List<DncLead>(DncLeads);
                }
            }
        }
    }

    private async Task Delete(int id)
    {
        var confirmed = await dialogService.Confirm(
            "Are you sure you want to delete this DNC lead?",
            "Delete Confirmation",
            new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

        if (confirmed is not true) return;

        using var context = contextFactory.CreateDbContext();
        await context.DncLeads.Where(x => x.Id == id).ExecuteDeleteAsync();
        DncLeads = DncLeads?.Where(x => x.Id != id).ToList();
    }

    private sealed class InputModel
    {
        public string Phone { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
```

- [ ] **Step 2: Verify build**

```
dotnet build
```
Expected: `Build succeeded` with 0 errors.

- [ ] **Step 3: Commit**

```bash
git add BlazorPortal/Components/Pages/DncLeadPages/
git commit -m "feat: add DncLead index page with search"
```

---

### Task 5: Create the Create Side Panel (razor)

**Files:**
- Create: `BlazorPortal/Components/Pages/DncLeadPages/CreatePage.razor`

Note: No `@page` directive — this is a dialog component, not a routable page.

- [ ] **Step 1: Create the razor file**

```razor
@inject IDbContextFactory<ApplicationDbContext> contextFactory
@inject DialogService DialogService

<EditForm Model="Input" OnValidSubmit="Save">
    <DataAnnotationsValidator />
    <ValidationSummary class="text-danger" />

    <RadzenStack Gap="1rem">

        <RadzenFieldset Text="Contact Info">
            <RadzenStack Gap="0.5rem">
                <RadzenRow AlignItems="AlignItems.Center">
                    <RadzenColumn Size="4"><RadzenLabel Text="Phone" /></RadzenColumn>
                    <RadzenColumn Size="8">
                        <RadzenTextBox @bind-Value="Input.Phone" class="w-100" />
                        <ValidationMessage For="() => Input.Phone" class="text-danger" />
                    </RadzenColumn>
                </RadzenRow>
                <RadzenRow AlignItems="AlignItems.Center">
                    <RadzenColumn Size="4"><RadzenLabel Text="First Name" /></RadzenColumn>
                    <RadzenColumn Size="8">
                        <RadzenTextBox @bind-Value="Input.FirstName" class="w-100" />
                    </RadzenColumn>
                </RadzenRow>
                <RadzenRow AlignItems="AlignItems.Center">
                    <RadzenColumn Size="4"><RadzenLabel Text="Last Name" /></RadzenColumn>
                    <RadzenColumn Size="8">
                        <RadzenTextBox @bind-Value="Input.LastName" class="w-100" />
                    </RadzenColumn>
                </RadzenRow>
                <RadzenRow AlignItems="AlignItems.Center">
                    <RadzenColumn Size="4"><RadzenLabel Text="Email" /></RadzenColumn>
                    <RadzenColumn Size="8">
                        <RadzenTextBox @bind-Value="Input.Email" class="w-100" />
                    </RadzenColumn>
                </RadzenRow>
            </RadzenStack>
        </RadzenFieldset>

        <RadzenFieldset Text="Address">
            <RadzenStack Gap="0.5rem">
                <RadzenRow AlignItems="AlignItems.Center">
                    <RadzenColumn Size="4"><RadzenLabel Text="Address" /></RadzenColumn>
                    <RadzenColumn Size="8">
                        <RadzenTextBox @bind-Value="Input.Address1" class="w-100" />
                    </RadzenColumn>
                </RadzenRow>
                <RadzenRow AlignItems="AlignItems.Center">
                    <RadzenColumn Size="4"><RadzenLabel Text="City" /></RadzenColumn>
                    <RadzenColumn Size="8">
                        <RadzenTextBox @bind-Value="Input.City" class="w-100" />
                    </RadzenColumn>
                </RadzenRow>
                <RadzenRow AlignItems="AlignItems.Center">
                    <RadzenColumn Size="4"><RadzenLabel Text="State" /></RadzenColumn>
                    <RadzenColumn Size="8">
                        <RadzenTextBox @bind-Value="Input.State" class="w-100" />
                    </RadzenColumn>
                </RadzenRow>
                <RadzenRow AlignItems="AlignItems.Center">
                    <RadzenColumn Size="4"><RadzenLabel Text="Postal Code" /></RadzenColumn>
                    <RadzenColumn Size="8">
                        <RadzenTextBox @bind-Value="Input.PostalCode" class="w-100" />
                    </RadzenColumn>
                </RadzenRow>
            </RadzenStack>
        </RadzenFieldset>

        <RadzenStack Orientation="Orientation.Horizontal" Gap="0.5rem">
            <RadzenButton ButtonType="ButtonType.Submit" Text="Save" ButtonStyle="ButtonStyle.Primary" />
            <RadzenButton ButtonType="ButtonType.Button" Text="Cancel" ButtonStyle="ButtonStyle.Light" Click="Cancel" />
        </RadzenStack>

    </RadzenStack>
</EditForm>
```

---

### Task 6: Create the Create Side Panel (code-behind)

**Files:**
- Create: `BlazorPortal/Components/Pages/DncLeadPages/CreatePage.razor.cs`

- [ ] **Step 1: Create the code-behind file**

```csharp
using BlazorPortal.Entities;

namespace BlazorPortal.Components.Pages.DncLeadPages;

public partial class CreatePage
{
    private DncLead Input { get; set; } = new();

    private async Task Save()
    {
        using var context = contextFactory.CreateDbContext();
        await context.DncLeads.AddAsync(Input);
        await context.SaveChangesAsync();
        DialogService.CloseSide(Input.Id);
    }

    private void Cancel() => DialogService.CloseSide(false);
}
```

- [ ] **Step 2: Verify build**

```
dotnet build
```
Expected: `Build succeeded` with 0 errors.

- [ ] **Step 3: Commit**

```bash
git add BlazorPortal/Components/Pages/DncLeadPages/CreatePage.razor BlazorPortal/Components/Pages/DncLeadPages/CreatePage.razor.cs
git commit -m "feat: add DncLead create side panel"
```

---

### Task 7: Create the Edit Side Panel (razor)

**Files:**
- Create: `BlazorPortal/Components/Pages/DncLeadPages/EditPage.razor`

Note: No `@page` directive — dialog component only.

- [ ] **Step 1: Create the razor file**

```razor
@inject IDbContextFactory<ApplicationDbContext> contextFactory
@inject DialogService DialogService

@if (Input is null)
{
    <em>Loading...</em>
}
else
{
    <EditForm Model="Input" OnValidSubmit="Save">
        <DataAnnotationsValidator />
        <ValidationSummary class="text-danger" />

        <RadzenStack Gap="1rem">

            <RadzenFieldset Text="Contact Info">
                <RadzenStack Gap="0.5rem">
                    <RadzenRow AlignItems="AlignItems.Center">
                        <RadzenColumn Size="4"><RadzenLabel Text="Phone" /></RadzenColumn>
                        <RadzenColumn Size="8">
                            <RadzenTextBox @bind-Value="Input.Phone" class="w-100" />
                            <ValidationMessage For="() => Input.Phone" class="text-danger" />
                        </RadzenColumn>
                    </RadzenRow>
                    <RadzenRow AlignItems="AlignItems.Center">
                        <RadzenColumn Size="4"><RadzenLabel Text="First Name" /></RadzenColumn>
                        <RadzenColumn Size="8">
                            <RadzenTextBox @bind-Value="Input.FirstName" class="w-100" />
                        </RadzenColumn>
                    </RadzenRow>
                    <RadzenRow AlignItems="AlignItems.Center">
                        <RadzenColumn Size="4"><RadzenLabel Text="Last Name" /></RadzenColumn>
                        <RadzenColumn Size="8">
                            <RadzenTextBox @bind-Value="Input.LastName" class="w-100" />
                        </RadzenColumn>
                    </RadzenRow>
                    <RadzenRow AlignItems="AlignItems.Center">
                        <RadzenColumn Size="4"><RadzenLabel Text="Email" /></RadzenColumn>
                        <RadzenColumn Size="8">
                            <RadzenTextBox @bind-Value="Input.Email" class="w-100" />
                        </RadzenColumn>
                    </RadzenRow>
                </RadzenStack>
            </RadzenFieldset>

            <RadzenFieldset Text="Address">
                <RadzenStack Gap="0.5rem">
                    <RadzenRow AlignItems="AlignItems.Center">
                        <RadzenColumn Size="4"><RadzenLabel Text="Address" /></RadzenColumn>
                        <RadzenColumn Size="8">
                            <RadzenTextBox @bind-Value="Input.Address1" class="w-100" />
                        </RadzenColumn>
                    </RadzenRow>
                    <RadzenRow AlignItems="AlignItems.Center">
                        <RadzenColumn Size="4"><RadzenLabel Text="City" /></RadzenColumn>
                        <RadzenColumn Size="8">
                            <RadzenTextBox @bind-Value="Input.City" class="w-100" />
                        </RadzenColumn>
                    </RadzenRow>
                    <RadzenRow AlignItems="AlignItems.Center">
                        <RadzenColumn Size="4"><RadzenLabel Text="State" /></RadzenColumn>
                        <RadzenColumn Size="8">
                            <RadzenTextBox @bind-Value="Input.State" class="w-100" />
                        </RadzenColumn>
                    </RadzenRow>
                    <RadzenRow AlignItems="AlignItems.Center">
                        <RadzenColumn Size="4"><RadzenLabel Text="Postal Code" /></RadzenColumn>
                        <RadzenColumn Size="8">
                            <RadzenTextBox @bind-Value="Input.PostalCode" class="w-100" />
                        </RadzenColumn>
                    </RadzenRow>
                </RadzenStack>
            </RadzenFieldset>

            <RadzenStack Orientation="Orientation.Horizontal" Gap="0.5rem">
                <RadzenButton ButtonType="ButtonType.Submit" Text="Save" ButtonStyle="ButtonStyle.Primary" />
                <RadzenButton ButtonType="ButtonType.Button" Text="Cancel" ButtonStyle="ButtonStyle.Light" Click="Cancel" />
            </RadzenStack>

        </RadzenStack>
    </EditForm>
}
```

---

### Task 8: Create the Edit Side Panel (code-behind)

**Files:**
- Create: `BlazorPortal/Components/Pages/DncLeadPages/EditPage.razor.cs`

- [ ] **Step 1: Create the code-behind file**

```csharp
using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.DncLeadPages;

public partial class EditPage
{
    [Parameter] public int DncLeadId { get; set; }

    private DncLead? Input { get; set; }

    protected override async Task OnInitializedAsync()
    {
        using var context = contextFactory.CreateDbContext();
        Input = await context.DncLeads.FindAsync(DncLeadId);
    }

    private async Task Save()
    {
        if (Input is null) return;

        using var context = contextFactory.CreateDbContext();
        context.Entry(Input).State = EntityState.Modified;
        await context.SaveChangesAsync();
        DialogService.CloseSide(Input.Id);
    }

    private void Cancel() => DialogService.CloseSide(false);
}
```

- [ ] **Step 2: Verify build**

```
dotnet build
```
Expected: `Build succeeded` with 0 errors.

- [ ] **Step 3: Commit**

```bash
git add BlazorPortal/Components/Pages/DncLeadPages/EditPage.razor BlazorPortal/Components/Pages/DncLeadPages/EditPage.razor.cs
git commit -m "feat: add DncLead edit side panel"
```

---

### Task 9: Add Nav Menu Item

**Files:**
- Modify: `BlazorPortal/Components/Layout/NavMenu.razor`

- [ ] **Step 1: Find the Leads section (around line 33) and add DNC item**

Find this block inside `<AuthorizeView Roles="Super Admin">`:
```razor
            <RadzenMenuItem Text="Leads" Icon="stacks">
                <RadzenMenuItem Text="Add New Lead" Path="LeadCreate" Icon="add" />
                <RadzenMenuItem Text="Transfer Lead" Path="LeadTransfer" Icon="send" />
                <RadzenMenuItem Text="Search Leads" Path="LeadIndex" Icon="list" />
                <RadzenMenuItem Text="Disposition Leads" Path="DispositionLead" Icon="checkbook" />
                <RadzenMenuItem Text="Recent Leads" Path="RecentLead" Icon="schedule" />
                <RadzenMenuItem Text="Lead Settings" Path="LeadSetting" Icon="tune" />
            </RadzenMenuItem>
```

Add one new item at the end of the Leads section, before the closing `</RadzenMenuItem>`:
```razor
                <RadzenMenuItem Text="DNC" Path="DncLeadIndex" Icon="block" />
```

Result:
```razor
            <RadzenMenuItem Text="Leads" Icon="stacks">
                <RadzenMenuItem Text="Add New Lead" Path="LeadCreate" Icon="add" />
                <RadzenMenuItem Text="Transfer Lead" Path="LeadTransfer" Icon="send" />
                <RadzenMenuItem Text="Search Leads" Path="LeadIndex" Icon="list" />
                <RadzenMenuItem Text="Disposition Leads" Path="DispositionLead" Icon="checkbook" />
                <RadzenMenuItem Text="Recent Leads" Path="RecentLead" Icon="schedule" />
                <RadzenMenuItem Text="Lead Settings" Path="LeadSetting" Icon="tune" />
                <RadzenMenuItem Text="DNC" Path="DncLeadIndex" Icon="block" />
            </RadzenMenuItem>
```

- [ ] **Step 2: Verify build**

```
dotnet build
```
Expected: `Build succeeded` with 0 errors.

- [ ] **Step 3: Commit**

```bash
git add BlazorPortal/Components/Layout/NavMenu.razor
git commit -m "feat: add DNC link under Leads nav menu"
```

---

### Task 10: Final Verification

- [ ] **Step 1: Run the app**

```
dotnet run --project BlazorPortal
```

- [ ] **Step 2: Manually verify all operations**

1. Login as Super Admin
2. Open Leads menu → click "DNC" → `/DncLeadIndex` loads with empty grid
3. Click "Add New DNC Lead" → right side panel opens with Contact Info + Address fieldsets
4. Fill Phone (required) + any other fields → click Save → panel closes, new record appears at top of grid
5. Click edit (pencil) on a row → side panel opens pre-filled → change a field → Save → grid updates in place
6. Click delete (trash) on a row → confirm dialog appears → click "Yes, Delete" → row removed from grid
7. Use Phone search → filtered results
8. Use Name search → filtered by FirstName or LastName

- [ ] **Step 3: Final commit (if any fixes made)**

```bash
git add -A
git commit -m "feat: complete DncLeads CRUD with sidebar pattern"
```
