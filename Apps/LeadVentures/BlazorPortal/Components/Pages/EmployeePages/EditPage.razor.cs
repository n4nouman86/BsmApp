using BlazorPortal.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace BlazorPortal.Components.Pages.EmployeePages;

public partial class EditPage
{
    [Parameter]
    public int Id { get; set; }

    private Employee? Input { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        Input = await db.Employees.FindAsync(Id);
    }

    private async Task Save()
    {
        if (Input is null) return;

        await using var db = await DbFactory.CreateDbContextAsync();
        db.Entry(Input).State = EntityState.Modified;
        await db.SaveChangesAsync();

        DialogService.CloseSide(Input.Id);
    }

    private void Cancel() => DialogService.CloseSide(false);
}
