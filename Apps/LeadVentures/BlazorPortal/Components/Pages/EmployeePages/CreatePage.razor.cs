using BlazorPortal.Entities;
using Radzen;

namespace BlazorPortal.Components.Pages.EmployeePages;

public partial class CreatePage
{
    private Employee Input { get; set; } = new() { StartDate = DateTime.Today, IsActive = true };

    private async Task Save()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        await db.Employees.AddAsync(Input);
        await db.SaveChangesAsync();

        DialogService.CloseSide(Input.Id);
    }

    private void Cancel() => DialogService.CloseSide(false);
}
